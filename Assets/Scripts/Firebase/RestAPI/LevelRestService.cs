using UnityEngine;
using Proyecto26;
using Firebase.Auth;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class LevelRestService : MonoBehaviour
{
	public static LevelRestService Instance;

	private const string CLOUD_NAME = "dbl7f0vfr";
	private const string UPLOAD_PRESET = "Matlab_level_thumbnails";

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

	// UPLOAD LEVEL

	public async Task UploadLevel(SceneData sceneData, Texture2D thumbnail, Action<float> onProgress = null)
	{
		onProgress?.Invoke(0f);

		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null)
		{
			Debug.LogError("No authenticated user.");
			return;
		}

		string token = await user.TokenAsync(true);

		bool isNew = string.IsNullOrEmpty(sceneData.uploadId);

		if (isNew)
			sceneData.uploadId = Guid.NewGuid().ToString();

		string documentUrl = FirestoreRestConfig.GetDocumentUrl("levels", sceneData.uploadId);

		ThumbnailUploadResult thumbnailResult = null;

		if (thumbnail != null)
			thumbnailResult = await UploadThumbnailAsync(thumbnail, sceneData.uploadId, p => onProgress?.Invoke(p * .6f));
		else
			onProgress?.Invoke(.6f);

		onProgress?.Invoke(.65f);

		if (isNew)
		{
			var levelData = ConstructLevelData(sceneData, user.UserId, thumbnailResult);
			var firestoreFields = FirestoreSerializer.SerializeRoot(levelData);

			firestoreFields["createdAt"] = new Dictionary<string, object>
			{
				{"timestampValue", DateTime.UtcNow.ToString("o") }
			};
			firestoreFields["updatedAt"] = new Dictionary<string, object>
			{
				{"timestampValue", DateTime.UtcNow.ToString("o") }
			};

			await RestClient.Patch(new RequestHelper
			{
				Uri = documentUrl,
				Headers = FirestoreRestConfig.GetAuthHeader(token),
				BodyString = JsonConvert.SerializeObject(new { fields = firestoreFields }),
				ContentType = "application/json"
			}).AsTask();

			onProgress?.Invoke(.85f);

			await IncrementFields("users", user.UserId, "uploadedLevelsCount", 1);
			onProgress?.Invoke(.95f);
		}
		else
		{
			var fields = new Dictionary<string, object>
			{
				{"sceneData", FirestoreSerializer.SerializeField(sceneData) },
				{"updatedAt", new {timestampValue = DateTime.UtcNow.ToString("o")} }
			};

			var updateMaskFields = new List<string> { "sceneData", "updatedAt" };

			if (thumbnailResult != null)
			{
				fields["thumbnailPath"] = new Dictionary<string, object>
				{
					{ "stringValue", thumbnailResult.secure_url }
				};

				fields["thumbnailPublicId"] = new Dictionary<string, object>
				{
					{ "stringValue", thumbnailResult.public_id }
				};

				updateMaskFields.Add("thumbnailPath");
				updateMaskFields.Add("thumbnailPublicId");
			}

			string updateMaskQuery = string.Join("&updateMask.fieldPaths=", updateMaskFields);

			await RestClient.Patch(new RequestHelper
			{
				Uri = documentUrl + $"?updateMask.fieldPaths={updateMaskQuery}",
				Headers = FirestoreRestConfig.GetAuthHeader(token),
				BodyString = JsonConvert.SerializeObject(new { fields } ),
				ContentType = "application/json"
			}).AsTask();

			onProgress?.Invoke(.95f);
		}

		onProgress?.Invoke(1f);
	}

	// FETCH LEVEL

	public async Task<List<LevelData>> GetAllLevels()
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null)
		{
			Debug.LogError("No authenticated user.");
			return null;
		}

		string token = await user.TokenAsync(true);

		string url = FirestoreRestConfig.GetDocumentUrl("levels");

		var response = await RestClient.Get(new RequestHelper
		{
			Uri = url,
			Headers = FirestoreRestConfig.GetAuthHeader(token)
		}).AsTask();

		JObject json = JObject.Parse(response.Text);

		if (!json.ContainsKey("documents"))
			return new List<LevelData>();

		var documents = (JArray)json["documents"];

		List<LevelData> levels = new List<LevelData>();

		foreach (var doc in documents)
		{
			LevelData level = FirestoreDeserializer.DeserializeDocument<LevelData>((JObject)doc);

			if (!level.isDeleted)
				levels.Add(level);
		}

		return levels;
	}

	//LIKE LEVEL

	public async Task ToggleLike(string levelId)
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null)
		{
			Debug.LogError("No authenticated user.");
			return;
		}

		string token = await user.TokenAsync(true);

		string likeDocId = $"{user.UserId}_{levelId}";
		string likeDocUrl = FirestoreRestConfig.GetDocumentUrl("likes", likeDocId);

		bool alreadyLiked = await LikeExists(likeDocUrl, token);

		if (alreadyLiked)
		{
			await Unlike(levelId, likeDocUrl, token, user.UserId);
			Debug.Log("Unliked");
		}
		else
		{
			await Like(levelId, likeDocUrl, token, user.UserId);
			Debug.Log("Liked");
		}
	}

	private async Task<bool> LikeExists(string likeDocUrl, string token)
	{
		try
		{
			await RestClient.Get(new RequestHelper
			{
				Uri = likeDocUrl,
				Headers = FirestoreRestConfig.GetAuthHeader(token)
			}).AsTask();

			return true;
		}
		catch
		{
			return false;
		}
	}

	private async Task Like(string levelId, string likeDocUrl, string token, string userId)
	{
		var likeData = new
		{
			fields = new Dictionary<string, object>
			{
				{"userId", new Dictionary<string, object>{{"stringValue", userId}} },
				{"levelId", new Dictionary<string, object>{{"stringValue", levelId}} },
				{"createdAt", new Dictionary<string, object>{{"timestampValue", DateTime.UtcNow.ToString("o")}} }
			}
		};

		await RestClient.Patch(new RequestHelper
		{
			Uri = likeDocUrl,
			Headers = FirestoreRestConfig.GetAuthHeader(token),
			BodyString = JsonConvert.SerializeObject(likeData),
			ContentType = "application/json"
		}).AsTask();

		await IncrementFields("levels", levelId, "likesCount", 1);
		await IncrementFields("users", userId, "likedLevelsCount", 1);
	}

	private async Task Unlike(string levelId, string likeDocUrl, string token, string userId)
	{
		await RestClient.Delete(new RequestHelper
		{
			Uri = likeDocUrl,
			Headers = FirestoreRestConfig.GetAuthHeader(token)
		}).AsTask();

		await IncrementFields("levels", levelId, "likesCount", -1);
		await IncrementFields("users", userId, "likedLevelsCount", -1);
	}

	public async Task<bool> HasCurrentUserLiked(string levelId)
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null)
		{
			Debug.LogError("No authenticated user.");
			return false;
		}

		string token = await user.TokenAsync(true);

		string likeDocId = $"{user.UserId}_{levelId}";
		string likeDocUrl = FirestoreRestConfig.GetDocumentUrl("likes", likeDocId);

		return await LikeExists(likeDocUrl, token);
	}

	// CLOUDINARY FUNCTION

	private async Task<ThumbnailUploadResult> UploadThumbnailAsync(Texture2D texture, string uploadId, Action<float> onProgress = null)
	{
		string publicId = $"level_thumbnails/{uploadId}_{DateTime.UtcNow.Ticks}";

		byte[] imageBytes = texture.EncodeToPNG();

		WWWForm form = new WWWForm();
		form.AddBinaryData("file", imageBytes, $"thumbnail.png", "image/png");
		form.AddField("upload_preset", UPLOAD_PRESET);
		form.AddField("public_id", publicId);

		string url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/image/upload";

		using (UnityWebRequest request = UnityWebRequest.Post(url, form))
		{
			var op = request.SendWebRequest();

			while (!op.isDone)
			{
				onProgress?.Invoke(request.uploadProgress);
				await Task.Yield();
			}

			onProgress?.Invoke(1f);

			if (request.result != UnityWebRequest.Result.Success)
				throw new Exception(request.error);

			string json = request.downloadHandler.text;

			ThumbnailUploadResult response = JsonUtility.FromJson<ThumbnailUploadResult>(json);

			return response;
		};
	}

	public async Task<Texture2D> LoadTextureAsync(string imageUrl)
	{
		if (string.IsNullOrEmpty(imageUrl))
			return null;

		using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
		{
			var op = request.SendWebRequest();

			while (!op.isDone)
				await Task.Yield();

			if (request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Failed to load thumbnail: {request.error}");
				return null;
			}

			return DownloadHandlerTexture.GetContent(request);
		}
	}

	// HELPER FUNCTION

	private LevelData ConstructLevelData(SceneData sceneData, string userId, ThumbnailUploadResult thumbnailResult)
	{
		return new LevelData
		{
			uploadId = sceneData.uploadId,
			levelName = sceneData.levelName,
			authorId = userId,
			authorName = "", // you can fetch from user document later

			//createdAt = DateTime.UtcNow.ToString("o"),
			//updatedAt = DateTime.UtcNow.ToString("o"),

			likesCount = 0,

			thumbnailPath = thumbnailResult?.secure_url ?? "",
			thumbnailPublicId = thumbnailResult?.public_id ?? "",

			sceneData = sceneData,

			isDeleted = false,
			deletedAt = null
		};
	}

	private async Task IncrementFields(string collection, string docId, string fieldName, int amount)
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null)
		{
			Debug.LogError("No authenticated user.");
			return;
		}

		string token = await user.TokenAsync(true);

		int currentValue = await GetIntField(collection, docId, fieldName);
		int newValue = Mathf.Max(0, currentValue + amount);

		string url = FirestoreRestConfig.GetDocumentUrl(collection, docId) + $"?updateMask.fieldPaths={fieldName}";

		var body = new
		{
			fields = new Dictionary<string, object>
			{
				{ fieldName, new Dictionary<string, object> {{"integerValue", newValue.ToString()}} }
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

	public async Task<int> GetIntField(string collection, string documentId, string fieldName)
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null)
		{
			Debug.LogError("No authenticated user.");
			return 0;
		}

		string token = await user.TokenAsync(true);

		string url = FirestoreRestConfig.GetDocumentUrl(collection, documentId)
					 + $"?mask.fieldPaths={fieldName}";

		var response = await RestClient.Get(new RequestHelper
		{
			Uri = url,
			Headers = FirestoreRestConfig.GetAuthHeader(token)
		}).AsTask();

		JObject json = JObject.Parse(response.Text);

		if (json["fields"]?[fieldName]?["integerValue"] != null)
		{
			return Convert.ToInt32(
				json["fields"][fieldName]["integerValue"].ToString()
			);
		}

		return 0;
	}
}

public class ThumbnailUploadResult
{
	public string public_id;
	public string secure_url;
}

[Serializable]
public class LevelData
{
	public string uploadId;
	public string levelName;
	public string authorId;
	public string authorName;

	public string createdAt;
	public string updatedAt;

	public int likesCount;

	public string thumbnailPath;
	public string thumbnailPublicId;

	public SceneData sceneData;

	public bool isDeleted;
	public string deletedAt;
}
