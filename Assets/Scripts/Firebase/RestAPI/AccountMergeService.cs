using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Proyecto26;

public static class AccountMergeService
{
	public static async Task MergeAnonymousDataIntoAccount(string oldUid, string oldToken, string newUid, string newToken)
	{
		var levelIds = await GetLevelIdsForAuthor(oldUid, oldToken);
		foreach (var levelId in levelIds)
			await ReassignLevelAuthor(levelId, newUid, oldToken);

		var likedLevelIds = await GetLikedLevelIds(oldUid, oldToken);
		foreach (var levelId in likedLevelIds)
			await RecreateLikeUnderNewUid(oldUid, newUid, levelId, oldToken, newToken);
	}

	private static async Task<List<string>> GetLevelIdsForAuthor(string uid, string token)
	{
		string url = FirestoreRestConfig.GetBaseUrl() + ":runQuery";
		var body = new
		{
			structuredQuery = new
			{
				from = new[] { new { collectionId = "levels" } },
				where = new { fieldFilter = new { field = new { fieldPath = "authorId" }, op = "EQUAL", value = new { stringValue = uid } } }
			}
		};

		var response = await RestClient.Post(new RequestHelper
		{
			Uri = url,
			Headers = FirestoreRestConfig.GetAuthHeader(token),
			BodyString = JsonConvert.SerializeObject(body),
			ContentType = "application/json"
		}).AsTask();

		var results = JArray.Parse(response.Text);
		var ids = new List<string>();
		foreach (var r in results)
		{
			string name = r["document"]?["name"]?.ToString();
			if (!string.IsNullOrEmpty(name))
				ids.Add(name.Substring(name.LastIndexOf('/') + 1));
		}
		return ids;
	}

	private static async Task ReassignLevelAuthor(string levelId, string newUid, string token)
	{
		string url = FirestoreRestConfig.GetDocumentUrl("levels", levelId) + "?updateMask.fieldPaths=authorId";
		var body = new { fields = new Dictionary<string, object> { { "authorId", new Dictionary<string, object> { { "stringValue", newUid } } } } };

		await RestClient.Patch(new RequestHelper
		{
			Uri = url,
			Headers = FirestoreRestConfig.GetAuthHeader(token),
			BodyString = JsonConvert.SerializeObject(body),
			ContentType = "application/json"
		}).AsTask();
	}

	private static async Task<List<string>> GetLikedLevelIds(string uid, string token)
	{
		string url = FirestoreRestConfig.GetBaseUrl() + ":runQuery";
		var body = new
		{
			structuredQuery = new
			{
				from = new[] { new { collectionId = "likes" } },
				where = new { fieldFilter = new { field = new { fieldPath = "userId" }, op = "EQUAL", value = new { stringValue = uid } } }
			}
		};

		var response = await RestClient.Post(new RequestHelper
		{
			Uri = url,
			Headers = FirestoreRestConfig.GetAuthHeader(token),
			BodyString = JsonConvert.SerializeObject(body),
			ContentType = "application/json"
		}).AsTask();

		var results = JArray.Parse(response.Text);
		var ids = new List<string>();
		foreach (var r in results)
		{
			var levelId = r["document"]?["fields"]?["levelId"]?["stringValue"]?.ToString();
			if (!string.IsNullOrEmpty(levelId))
				ids.Add(levelId);
		}
		return ids;
	}

	private static async Task RecreateLikeUnderNewUid(string oldUid, string newUid, string levelId, string oldToken, string newToken)
	{
		string newLikeUrl = FirestoreRestConfig.GetDocumentUrl("likes", $"{newUid}_{levelId}");

		try
		{
			await RestClient.Get(new RequestHelper { Uri = newLikeUrl, Headers = FirestoreRestConfig.GetAuthHeader(newToken) }).AsTask();
			return; // new account already liked it, nothing to merge
		}
		catch (RequestException e) when (e.StatusCode == 404) { }

		var body = new
		{
			fields = new Dictionary<string, object>
			{
				{ "userId", new Dictionary<string, object> { { "stringValue", newUid } } },
				{ "levelId", new Dictionary<string, object> { { "stringValue", levelId } } },
				{ "createdAt", new Dictionary<string, object> { { "timestampValue", System.DateTime.UtcNow.ToString("o") } } }
			}
		};

		await RestClient.Patch(new RequestHelper
		{
			Uri = newLikeUrl,
			Headers = FirestoreRestConfig.GetAuthHeader(newToken),
			BodyString = JsonConvert.SerializeObject(body),
			ContentType = "application/json"
		}).AsTask();

		await RestClient.Delete(new RequestHelper
		{
			Uri = FirestoreRestConfig.GetDocumentUrl("likes", $"{oldUid}_{levelId}"),
			Headers = FirestoreRestConfig.GetAuthHeader(oldToken)
		}).AsTask();
	}
}