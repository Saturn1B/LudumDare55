using UnityEngine;
using Firebase;
using Firebase.Auth;
using System.Threading.Tasks;

public class AuthenticationManager : MonoBehaviour
{
	public static FirebaseAuth auth { get; private set; }

	private async void Start()
	{
		DontDestroyOnLoad(gameObject);

		await InitializeFirebase();
		await SignInAnonymously();
		await UserRestService.Instance.CreateUserIfNeeded();
	}

	private async Task InitializeFirebase()
	{
		var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

		if(dependencyStatus != DependencyStatus.Available)
		{
			Debug.LogError("Firebase dependencies failed");
			return;
		}

		auth = FirebaseAuth.DefaultInstance;
	}

	private async Task SignInAnonymously()
	{
		if(auth.CurrentUser != null)
		{
			Debug.Log($"Already signed in: {auth.CurrentUser.UserId}");
			return;
		}

		var result = await auth.SignInAnonymouslyAsync();
		Debug.Log($"Signed in anonymously: {result.User.UserId}");
	}
}
