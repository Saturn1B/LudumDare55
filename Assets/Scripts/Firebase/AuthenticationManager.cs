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

	public static string CurrentUsername { get; private set; } = "";

	private async void Awake()
	{
		if (Instance != null && Instance != this) { Destroy(gameObject); return; }
		Instance = this;
		DontDestroyOnLoad(gameObject);

		await InitializeFirebase();

		if (SecureCredentialStore.TryGet(out string storedRefreshToken))
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

	public async Task<bool> EnsureGoogleSignedInAsync()
	{
		if (IsSignedIn && !IsAnonymous)
			return true;

		if (SecureCredentialStore.TryGet(out string storedRefreshToken) && await SilentSignInAsync(storedRefreshToken))
			return true;

		if (IsSignedIn && IsAnonymous)
			return await LinkAnonymousToGoogle();

		return await SignInWithGoogleFresh();
	}

	public async Task<string> RefreshUsernameAsync()
	{
		if (!IsSignedIn)
		{
			CurrentUsername = "";
			return CurrentUsername;
		}

		CurrentUsername = await UserRestService.Instance.GetUsername();
		return CurrentUsername;
	}

	public void SignOut()
	{
		auth.SignOut();

		SecureCredentialStore.Delete();

		CurrentUsername = "";

		Debug.Log("Signed out and forgot stored credentials.");
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
			SecureCredentialStore.Delete();
			return false;
		}

		Credential credential = GoogleAuthProvider.GetCredential(tokens.IdToken, tokens.AccessToken);
		var result = await auth.SignInWithCredentialAsync(credential);

		Debug.Log($"Silently restored Google session: {result.UserId}");

		StoreRefreshToken(tokens.RefreshToken);

		await UserRestService.Instance.CreateUserIfNeeded(tokens.DisplayName);
		await UserRestService.Instance.SyncGoogleUsernameIfNeeded(tokens.DisplayName);
		await UserRestService.Instance.RecalculateLikedLevelsCount();
		await RefreshUsernameAsync();

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
		await RefreshUsernameAsync();
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
			await RefreshUsernameAsync();
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
			await RefreshUsernameAsync();
			return true;
		}
	}

	private static void StoreRefreshToken(string refreshToken)
	{
		if (string.IsNullOrEmpty(refreshToken))
			return;

		SecureCredentialStore.Set(refreshToken);
	}
}