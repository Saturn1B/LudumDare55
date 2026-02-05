using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Text;
using Newtonsoft.Json;

public class FirestoreManager : MonoBehaviour
{
	public static FirestoreManager Instance { get; private set; }

	private FirebaseFirestore firestore;

	private const string CLOUD_NAME = "dbl7f0vfr";
	private const string UPLOAD_PRESET = "Matlab_level_thumbnails";

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this;
		DontDestroyOnLoad(gameObject);

		firestore = FirebaseFirestore.DefaultInstance;
	}

	public async Task UploadLevel(SceneData sceneData, Texture2D thumbnail = null)
	{
		if (string.IsNullOrEmpty(sceneData.uploadId))
			await FirstUpload(sceneData, thumbnail);
		else
			await UpdateUpload(sceneData, thumbnail);
	}

	private async Task FirstUpload(SceneData sceneData, Texture2D thumbnail = null)
	{
		string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

		DocumentReference levelRef = firestore.Collection("levels").Document();

		string uploadId = levelRef.Id;

		sceneData.uploadId = uploadId;

		string thumbnailUrl = null;
		if (thumbnail != null)
		{
			thumbnailUrl = await UploadThumbnailAsync(thumbnail, uploadId);
		}

		LevelData levelData = new LevelData
		{
			uploadId = uploadId,
			levelName = sceneData.levelName,
			authorId = userId,
			authorName = null,
			createdAt = Timestamp.GetCurrentTimestamp(),
			updatedAt = Timestamp.GetCurrentTimestamp(),
			likesCount = 0,
			thumbnailPath = thumbnailUrl,
			sceneData = sceneData
		};

		DocumentReference userRef = firestore.Collection("users").Document(userId);

		await firestore.RunTransactionAsync(async transaction =>
		{
			transaction.Set(levelRef, levelData);

			transaction.Update(userRef, new Dictionary<string, object>
			{
				{"uploadedLevelsCount", FieldValue.Increment(1) }
			});

			return Task.CompletedTask;
		});
	}

	private async Task UpdateUpload(SceneData sceneData, Texture2D thumbnail = null)
	{
		DocumentReference levelRef = firestore.Collection("levels").Document(sceneData.uploadId);

		Dictionary<string, object> updates = new Dictionary<string, object>
		{
			{"sceneData", sceneData },
			{"updatedAt", Timestamp.GetCurrentTimestamp() }
		};


		if (thumbnail != null)
		{
			string thumbnailUrl = await UploadThumbnailAsync(thumbnail, sceneData.uploadId);

			updates["thumbnailPath"] = thumbnailUrl;
		}

		await levelRef.UpdateAsync(updates);
	}

	public async Task<List<LevelData>> GetAllLevels()
	{
		QuerySnapshot snapshot = await firestore.Collection("levels").GetSnapshotAsync();

		List<LevelData> levels = new List<LevelData>();

		foreach (DocumentSnapshot doc in snapshot.Documents)
		{
			if (!doc.Exists)
				continue;

			LevelData level = doc.ConvertTo<LevelData>();
			levels.Add(level);
		}

		return levels;
	}

	public async Task<bool> HasUserLiked(string uploadId)
	{
		string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

		DocumentReference likeRef = firestore.Collection("levels").Document(uploadId).Collection("likes").Document(userId);

		DocumentSnapshot snapshot = await likeRef.GetSnapshotAsync();

		return snapshot.Exists;
	}

	public async Task ToggleLikeLevel(string uploadId)
	{
		string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

		DocumentReference levelRef = firestore.Collection("levels").Document(uploadId);

		DocumentReference levelLikeRef = levelRef.Collection("likes").Document(userId);

		DocumentReference userRef = firestore.Collection("users").Document(userId);

		DocumentReference userLikedLevelRef = userRef.Collection("likedLevels").Document(uploadId);

		await firestore.RunTransactionAsync(async transaction =>
		{
			DocumentSnapshot likeSnapshot = await transaction.GetSnapshotAsync(levelLikeRef);

			bool alreadyLiked = likeSnapshot.Exists;

			//Unlike
			if (alreadyLiked)
			{
				transaction.Delete(levelLikeRef);
				transaction.Delete(userLikedLevelRef);

				transaction.Update(levelRef, new Dictionary<string, object>
				{
					{"likesCount", FieldValue.Increment(-1) }
				});

				transaction.Update(userRef, new Dictionary<string, object>
				{
					{"likedLevelsCount", FieldValue.Increment(-1) }
				});
			}
			//Like
			else
			{
				transaction.Set(levelLikeRef, new Dictionary<string, object>
				{
					{"likedAt", Timestamp.GetCurrentTimestamp() }
				});

				transaction.Set(userLikedLevelRef, new Dictionary<string, object>
				{
					{"likedAt", Timestamp.GetCurrentTimestamp() }
				});

				transaction.Update(levelRef, new Dictionary<string, object>
				{
					{"likesCount", FieldValue.Increment(1) }
				});

				transaction.Update(userRef, new Dictionary<string, object>
				{
					{"likedLevelsCount", FieldValue.Increment(1) }
				});
			}
		});
	}

	public async Task<int> GetLikeCount(string uploadId)
	{
		DocumentSnapshot snapshot = await firestore.Collection("levels").Document(uploadId).GetSnapshotAsync();

		return snapshot.GetValue<int>("likesCount");
	}

	public IEnumerator UploadThumbnail(Texture2D texture, string uploadId, System.Action<string> onSuccess, System.Action<string> onError)
	{
		byte[] imageBytes = texture.EncodeToPNG();

		WWWForm form = new WWWForm();
		form.AddBinaryData("file", imageBytes, $"{uploadId}.png", "image/png");
		form.AddField("upload_preset", UPLOAD_PRESET);
		form.AddField("public_id", uploadId);
		form.AddField("folder", "level_thumbnails");

		string url = $"https://api.cloudinary.com/v1_1/{CLOUD_NAME}/image/upload";

		using (UnityWebRequest request = UnityWebRequest.Post(url, form))
		{
			yield return request.SendWebRequest();

			if(request.result != UnityWebRequest.Result.Success)
			{
				onError?.Invoke(request.error);
			}
			else
			{
				string json = request.downloadHandler.text;
				string secureUrl = ExtractSecureUrl(json);

				onSuccess?.Invoke(secureUrl);
			}
		}
	}

	private string ExtractSecureUrl(string json)
	{
		const string key = "\"secure_url\":\"";
		int start = json.IndexOf(key) + key.Length;
		int end = json.IndexOf("\"", start);
		return json.Substring(start, end - start);
	}

	private Task<string> UploadThumbnailAsync(Texture2D thumbnail, string uploadId)
	{
		var tcs = new TaskCompletionSource<string>();

		StartCoroutine(UploadThumbnail(
			thumbnail,
			uploadId,
			url => tcs.SetResult(url),
			error => tcs.SetException(new System.Exception(error))
		));

		return tcs.Task;
	}
}

[System.Serializable][FirestoreData]
public class LevelData
{
	[FirestoreProperty]
	public string uploadId { get; set; }
	[FirestoreProperty]
	public string levelName { get; set; }
	[FirestoreProperty]
	public string authorId { get; set; }
	[FirestoreProperty]
	public string authorName { get; set; }

	[FirestoreProperty]
	public Timestamp createdAt { get; set; }
	[FirestoreProperty]
	public Timestamp updatedAt { get; set; }

	[FirestoreProperty]
	public int likesCount { get; set; }

	[FirestoreProperty]
	public string thumbnailPath { get; set; }

	[FirestoreProperty]
	public SceneData sceneData { get; set; }
}

[System.Serializable][FirestoreData]
public class User
{
	[FirestoreProperty]
	public string userId { get; set; }
	[FirestoreProperty]
	public string userName { get; set; }

	[FirestoreProperty]
	public Timestamp createdAt { get; set; }

	[FirestoreProperty]
	public int uploadedLevelsCount { get; set; }
	[FirestoreProperty]
	public int likedLevelsCount { get; set; }
}
