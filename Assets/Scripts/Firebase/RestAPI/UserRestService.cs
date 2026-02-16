using UnityEngine;
using Proyecto26;
using Firebase.Auth;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public class UserRestService : MonoBehaviour
{
	public static UserRestService Instance;

	private void Awake()
	{
		if(Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);
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

		string getUrl = FirestoreRestConfig.GetDocumentUrl("users") + $"{userId}";

		try
		{
			await RestClient.Get(new RequestHelper
			{
				Uri = getUrl,
				Headers = FirestoreRestConfig.GetAuthHeader(token)
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

		string postUrl = FirestoreRestConfig.GetDocumentUrl("users") + $"?documentId={userId}";

		await RestClient.Post(new RequestHelper
		{
			Uri = postUrl,
			Headers = FirestoreRestConfig.GetAuthHeader(token),
			BodyString = JsonConvert.SerializeObject(body),
			ContentType = "application/json"
		}).AsTask();

		Debug.Log("User created succesfully");
	}

	public async Task RecalculateLikedLevelsCount()
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null)
		{
			Debug.LogError("No authenticated user.");
			return;
		}

		string token = await user.TokenAsync(true);

		string url = FirestoreRestConfig.GetBaseUrl() + ":runQuery";

		var body = new
		{
			structuredQuery = new
			{
				from = new[] { new { collectionId = "likes" } },
				where = new
				{
					fieldFilter = new
					{
						field = new { fieldPath = "userId" },
						op = "EQUAL",
						value = new { stringValue = user.UserId }
					}
				}
			}
		};

		var response = await RestClient.Post(new RequestHelper
		{
			Uri = url,
			Headers = FirestoreRestConfig.GetAuthHeader(token),
			BodyString = JsonConvert.SerializeObject(body),
			ContentType = "application/json"
		}).AsTask();

		JArray results = JArray.Parse(response.Text);

		int count = 0;

		foreach (var r in results)
		{
			if (r["document"] != null)
				count++;
		}

		await IncrementFieldExact("users", user.UserId, "likedLevelsCount", count);
	}

	// HELPER FUNCTION

	private async Task IncrementFieldExact(string collection, string documentId, string fieldName, int value)
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null)
		{
			Debug.LogError("No authenticated user.");
			return;
		}

		string token = await user.TokenAsync(true);

		string url = FirestoreRestConfig.GetDocumentUrl(collection, documentId)
			+ $"?updateMask.fieldPaths={fieldName}";

		var body = new
		{
			fields = new Dictionary<string, object>
			{
				{ fieldName, new Dictionary<string, object> {{"integerValue", value.ToString() }}}
			}
		};

		await RestClient.Patch(new RequestHelper
		{
			Uri = url,
			Headers = FirestoreRestConfig.GetAuthHeader(token),
			BodyString = JsonConvert.SerializeObject(body),
			ContentType = "application/json"
		}).AsTask();
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
