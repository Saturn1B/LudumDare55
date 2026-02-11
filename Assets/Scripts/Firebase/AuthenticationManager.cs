using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;

public class AuthenticationManager : MonoBehaviour
{
	public static FirebaseAuth auth { get; private set; }

	private async void Awake()
	{
		DontDestroyOnLoad(gameObject);

		while (!FirebaseInitializer.isReady)
			await Task.Yield();

		auth = FirebaseAuth.DefaultInstance;

		await SignInAnonymously();

		await CreateUser();
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

		await FirestoreManager.Instance.CleanupInvalidLevels();
	}

	private async Task CreateUser()
	{
		var firestore = FirebaseFirestore.DefaultInstance;
		string userId = auth.CurrentUser.UserId;

		DocumentReference userRef = firestore.Collection("users").Document(userId);

		var snapshot = await userRef.GetSnapshotAsync();

		if (!snapshot.Exists)
		{
			await userRef.SetAsync(new User
			{
				userId = userId,
				userName = null,
				createdAt = Timestamp.GetCurrentTimestamp(),
				uploadedLevelsCount = 0,
				likedLevelsCount = 0
			});
		}
	}
}
