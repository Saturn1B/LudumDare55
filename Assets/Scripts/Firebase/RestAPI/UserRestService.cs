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

		string getUrl = FirestoreRestConfig.GetDocumentUrl("users", userId);

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
		catch (RequestException e)
		{
			if (e.StatusCode == 404)
			{
				Debug.Log("User does not exist. Creating...");
			}
			else
			{
				Debug.LogError($"Unexpected error: {e.StatusCode}");
				throw;
			}
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

	public async Task RecalculateUploadedLevelsCount()
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null) return;

		string token = await user.TokenAsync(true);
		string url = FirestoreRestConfig.GetBaseUrl() + ":runQuery";

		var body = new
		{
			structuredQuery = new
			{
				from = new[] { new { collectionId = "levels" } },
				where = new { fieldFilter = new { field = new { fieldPath = "authorId" }, op = "EQUAL", value = new { stringValue = user.UserId } } }
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
			if (r["document"] != null) count++;

		await IncrementFieldExact("users", user.UserId, "uploadedLevelsCount", count);
	}

	public async Task CreateUserIfNeeded(string displayName = "")
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null)
		{
			Debug.LogError("No authenticated user.");
			return;
		}

		string token = await user.TokenAsync(true);
		string userId = user.UserId;

		string getUrl = FirestoreRestConfig.GetDocumentUrl("users", userId);

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
		catch (RequestException e)
		{
			if (e.StatusCode == 404)
			{
				Debug.Log("User does not exist. Creating...");
			}
			else
			{
				Debug.LogError($"Unexpected error: {e.StatusCode}");
				throw;
			}
		}

		var body = new
		{
			fields = new
			{
				userId = new { stringValue = userId },
				userName = new { stringValue = displayName ?? "" },
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

	// For accounts that were created back when userName was always "" (e.g. anonymous
	// users who've now linked Google) - fills it in the first time we have a real name.
	public async Task SyncGoogleUsernameIfNeeded(string displayName)
	{
		if (string.IsNullOrEmpty(displayName))
			return;

		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null) return;

		string token = await user.TokenAsync(true);
		string url = FirestoreRestConfig.GetDocumentUrl("users", user.UserId) + "?mask.fieldPaths=userName";

		var response = await RestClient.Get(new RequestHelper
		{
			Uri = url,
			Headers = FirestoreRestConfig.GetAuthHeader(token)
		}).AsTask();

		JObject json = JObject.Parse(response.Text);
		string currentName = json["fields"]?["userName"]?["stringValue"]?.ToString();

		if (!string.IsNullOrEmpty(currentName))
			return; // don't overwrite a name the player may have set themselves

		string patchUrl = FirestoreRestConfig.GetDocumentUrl("users", user.UserId) + "?updateMask.fieldPaths=userName";

		var body = new
		{
			fields = new Dictionary<string, object>
		{
			{ "userName", new Dictionary<string, object> { { "stringValue", displayName } } }
		}
		};

		await RestClient.Patch(new RequestHelper
		{
			Uri = patchUrl,
			Headers = FirestoreRestConfig.GetAuthHeader(token),
			BodyString = JsonConvert.SerializeObject(body),
			ContentType = "application/json"
		}).AsTask();
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
