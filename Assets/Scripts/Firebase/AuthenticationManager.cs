using UnityEngine;
using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;

public class AuthenticationManager : MonoBehaviour
{
	public static AuthenticationManager Instance { get; private set; }
	public static FirebaseAuth auth { get; private set; }

	public static bool IsSignedIn => auth?.CurrentUser != null;
	public static bool IsAnonymous => auth?.CurrentUser != null && auth.CurrentUser.IsAnonymous;

	private const string GoogleRefreshTokenKey = "GoogleRefreshToken";

	private async void Awake()
	{
		if (Instance != null && Instance != this) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);

		await InitializeFirebase();

		string storedRefreshToken = PlayerPrefs.GetString(GoogleRefreshTokenKey, "");
		if (!string.IsNullOrEmpty(storedRefreshToken))
			await SilentSignInAsync(storedRefreshToken);
	}

	private async Task InitializeFirebase()
	{
		var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

		if (dependencyStatus != DependencyStatus.Available)
		{
			Debug.LogError("Firebase dependencies failed");
			return;
		}

		auth = FirebaseAuth.DefaultInstance;
	}

	/// <summary>
	/// Call this right before opening the level editor (or any Google-gated feature).
	/// Silently restores a returning session if we have a stored refresh token, links
	/// a first-time anonymous session, or opens the interactive Google sign-in.
	/// </summary>
	public async Task<bool> EnsureGoogleSignedInAsync()
	{
		if (IsSignedIn && !IsAnonymous)
			return true;

		string storedRefreshToken = PlayerPrefs.GetString(GoogleRefreshTokenKey, "");
		if (!string.IsNullOrEmpty(storedRefreshToken) && await SilentSignInAsync(storedRefreshToken))
			return true;

		if (IsSignedIn && IsAnonymous)
			return await LinkAnonymousToGoogle();

		return await SignInWithGoogleFresh();
	}

	private async Task<bool> SilentSignInAsync(string refreshToken)
	{
		GoogleDesktopAuth.GoogleTokens tokens;

		try
		{
			tokens = await GoogleDesktopAuth.RefreshTokensAsync(refreshToken);
		}
		catch (Exception e)
		{
			Debug.LogWarning($"Silent Google sign-in failed, will need interactive sign-in: {e.Message}");
			PlayerPrefs.DeleteKey(GoogleRefreshTokenKey);
			return false;
		}

		Credential credential = GoogleAuthProvider.GetCredential(tokens.IdToken, tokens.AccessToken);
		var result = await auth.SignInWithCredentialAsync(credential);

		Debug.Log($"Silently restored Google session: {result.UserId}");

		StoreRefreshToken(tokens.RefreshToken);

		await UserRestService.Instance.CreateUserIfNeeded(tokens.DisplayName);
		await UserRestService.Instance.SyncGoogleUsernameIfNeeded(tokens.DisplayName);
		await UserRestService.Instance.RecalculateLikedLevelsCount();

		return true;
	}

	private async Task<bool> SignInWithGoogleFresh()
	{
		GoogleDesktopAuth.GoogleTokens tokens;

		try
		{
			tokens = await GoogleDesktopAuth.SignInAsync();
		}
		catch (Exception e)
		{
			Debug.LogError($"Google sign-in cancelled or failed: {e.Message}");
			return false;
		}

		Credential credential = GoogleAuthProvider.GetCredential(tokens.IdToken, tokens.AccessToken);
		var result = await auth.SignInWithCredentialAsync(credential);
		Debug.Log($"Signed in with Google: {result.UserId}");

		StoreRefreshToken(tokens.RefreshToken);

		await UserRestService.Instance.CreateUserIfNeeded(tokens.DisplayName);
		await UserRestService.Instance.SyncGoogleUsernameIfNeeded(tokens.DisplayName);
		await UserRestService.Instance.RecalculateLikedLevelsCount();
		return true;
	}

	private async Task<bool> LinkAnonymousToGoogle()
	{
		string oldAnonUid = auth.CurrentUser.UserId;
		string oldAnonToken = await auth.CurrentUser.TokenAsync(true);

		GoogleDesktopAuth.GoogleTokens tokens;

		try
		{
			tokens = await GoogleDesktopAuth.SignInAsync();
		}
		catch (Exception e)
		{
			Debug.LogError($"Google sign-in cancelled or failed: {e.Message}");
			return false;
		}

		Credential credential = GoogleAuthProvider.GetCredential(tokens.IdToken, tokens.AccessToken);

		try
		{
			var linkedUser = await auth.CurrentUser.LinkWithCredentialAsync(credential);
			Debug.Log($"Linked anonymous account to Google, UID unchanged: {linkedUser.User.UserId}");

			StoreRefreshToken(tokens.RefreshToken);

			await UserRestService.Instance.CreateUserIfNeeded(tokens.DisplayName);
			await UserRestService.Instance.SyncGoogleUsernameIfNeeded(tokens.DisplayName);
			return true;
		}
		catch (FirebaseAccountLinkException)
		{
			Debug.LogWarning("Google account already linked elsewhere. Merging anonymous data into it...");

			var newUser = await auth.SignInWithCredentialAsync(credential);
			string newToken = await newUser.TokenAsync(true);

			await AccountMergeService.MergeAnonymousDataIntoAccount(oldAnonUid, oldAnonToken, newUser.UserId, newToken);

			StoreRefreshToken(tokens.RefreshToken);

			await UserRestService.Instance.CreateUserIfNeeded(tokens.DisplayName);
			await UserRestService.Instance.SyncGoogleUsernameIfNeeded(tokens.DisplayName);
			await UserRestService.Instance.RecalculateLikedLevelsCount();
			await UserRestService.Instance.RecalculateUploadedLevelsCount();
			return true;
		}
	}

	private static void StoreRefreshToken(string refreshToken)
	{
		if (string.IsNullOrEmpty(refreshToken))
			return;

		PlayerPrefs.SetString(GoogleRefreshTokenKey, refreshToken);
		PlayerPrefs.Save();
	}
}