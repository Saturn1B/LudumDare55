using UnityEngine;
using Proyecto26;
using Firebase.Auth;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;

public class UserRestService : MonoBehaviour
{
	public static UserRestService Instance;

	private string projectId = "matlab-c258c";
	private string baseUrl;

	private void Awake()
	{
		if(Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		baseUrl = $"https://firestore.googleapis.com/v1/projects/{projectId}/databases/(default)/documents";
	}

	private Dictionary<string, string> AuthHeader(string token)
	{
		return new Dictionary<string, string>
		{
			{"Authorization", "Bearer " + token }
		};
	}

	public async Task CreateUserIfNeeded()
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if(user == null)
		{
			Debug.LogError("No authenticated user.");
			return;
		}

		string token = await user.TokenAsync(true);
		string userId = user.UserId;

		string getUrl = $"{baseUrl}/users/{userId}";

		try
		{
			await RestClient.Get(new RequestHelper
			{
				Uri = getUrl,
				Headers = AuthHeader(token)
			}).AsTask();

			Debug.Log("User already exists");
			return;
		}
		catch
		{
			Debug.Log("User does not exist. Creating...");
		}

		var body = new
		{
			fields = new
			{
				userId = new { stringValue = userId },
				userName = new { stringValue = "" },
				createdAt = new { timestampValue = System.DateTime.UtcNow.ToString("o") },
				uploadedLevelsCount = new { integerValue = "0" },
				likedLevelsCount = new { integerValue = "0" }
			}
		};

		string postUrl = $"{baseUrl}/users?documentId={userId}";

		await RestClient.Post(new RequestHelper
		{
			Uri = postUrl,
			Headers = AuthHeader(token),
			BodyString = JsonConvert.SerializeObject(body),
			ContentType = "application/json"
		}).AsTask();

		Debug.Log("User created succesfully");
	}
}

[System.Serializable]
public class User
{
	public string userId;
	public string userName;

	public string createdAt;

	public int uploadedLevelsCount;
	public int likedLevelsCount;
}
