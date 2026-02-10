using System;
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

public enum LevelSortType
{
	LIKED = 0,
	NEWEST = 1,
	UPDATED = 2
}

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

		ThumbnailUploadResult thumbnailResult = null;
		if (thumbnail != null)
		{
			thumbnailResult = await UploadThumbnailAsync(thumbnail, uploadId);
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
			thumbnailPath = thumbnailResult.secure_url,
			thumbnailPublicId = thumbnailResult.public_id,
			sceneData = sceneData,
			isDeleted = false
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

		DocumentSnapshot snap = await levelRef.GetSnapshotAsync();
		if (!snap.Exists || snap.GetValue<bool>("isDeleted"))
			throw new Exception("Cannot update a deleted level");

		Dictionary<string, object> updates = new Dictionary<string, object>
		{
			{"sceneData", sceneData },
			{"updatedAt", Timestamp.GetCurrentTimestamp() }
		};


		if (thumbnail != null)
		{
			ThumbnailUploadResult thumbnailResult = await UploadThumbnailAsync(thumbnail, sceneData.uploadId);

			updates["thumbnailPath"] = thumbnailResult.secure_url;
			updates["thumbnailPublicId"] = thumbnailResult.public_id;
		}

		await levelRef.UpdateAsync(updates);
	}

	public async Task<List<LevelData>> GetAllLevels(LevelSortType sortType)
	{
		await CleanupInvalidLevels();

		Query query = firestore.Collection("levels").WhereEqualTo("isDeleted", false);

		switch (sortType)
		{
			case LevelSortType.LIKED:
				query = query.OrderByDescending("likesCount");
				break;
			case LevelSortType.NEWEST:
				query = query.OrderByDescending("createdAt");
				break;
			case LevelSortType.UPDATED:
				query = query.OrderByDescending("updatedAt");
				break;
		}

		QuerySnapshot snapshot = await query.GetSnapshotAsync();

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

		DocumentSnapshot levelSnap = await levelRef.GetSnapshotAsync();
		if (!levelSnap.Exists || levelSnap.GetValue<bool>("isDeleted"))
			return;

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
				transaction.Update(levelRef, "likesCount", FieldValue.Increment(-1));
				transaction.Update(userRef, "likedLevelsCount", FieldValue.Increment(-1));
			}
			//Like
			else
			{
				transaction.Set(levelLikeRef, new {likedAt = Timestamp.GetCurrentTimestamp() });
				transaction.Set(userLikedLevelRef, new {likedAt = Timestamp.GetCurrentTimestamp() });
				transaction.Update(levelRef, "likesCount", FieldValue.Increment(1));
				transaction.Update(userRef, "likedLevelsCount", FieldValue.Increment(1));
			}
		});
	}

	public async Task<int> GetLikeCount(string uploadId)
	{
		DocumentSnapshot snapshot = await firestore.Collection("levels").Document(uploadId).GetSnapshotAsync();

		return snapshot.GetValue<int>("likesCount");
	}

	private async Task<ThumbnailUploadResult> UploadThumbnailAsync(Texture2D texture, string uploadId)
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
				await Task.Yield();

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

			if(request.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError($"Failed to load thumbnail: {request.error}");
				return null;
			}

			return DownloadHandlerTexture.GetContent(request);
		}
	}

	public async Task DeleteLevel(string uploadId)
	{
		string currentUserId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

		DocumentReference levelRef = firestore.Collection("levels").Document(uploadId);

		DocumentSnapshot levelSnap = await levelRef.GetSnapshotAsync();

		if (!levelSnap.Exists)
			throw new Exception("Level does not exist");

		string authorId = levelSnap.GetValue<string>("authorId");

		if (authorId != currentUserId)
			throw new Exception("User is not the author of this level");

		WriteBatch batch = firestore.StartBatch();

		batch.Update(levelRef, new Dictionary<string, object>
		{
			{"isDeleted", true },
			{"deletedAt", Timestamp.GetCurrentTimestamp() }
		});

		DocumentReference userRef = firestore.Collection("users").Document(currentUserId);

		batch.Update(userRef, "uploadedLevelsCount", FieldValue.Increment(-1));

		await batch.CommitAsync();
	}

	public async Task CleanupInvalidLevels()
	{
		string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

		DocumentReference userRef = firestore.Collection("users").Document(userId);

		QuerySnapshot likedLevels = await userRef.Collection("likedLevels").GetSnapshotAsync();

		WriteBatch batch = firestore.StartBatch();
		int removedCount = 0;

		foreach (DocumentSnapshot doc in likedLevels.Documents)
		{
			DocumentSnapshot levelSnap = await firestore.Collection("levels").Document(doc.Id).GetSnapshotAsync();

			if (!levelSnap.Exists || levelSnap.GetValue<bool>("isDeleted"))
			{
				batch.Delete(doc.Reference);
				removedCount++;
			}
		}

		if(removedCount > 0)
		{
			batch.Update(userRef, "likedLevelsCount", FieldValue.Increment(-removedCount));
			await batch.CommitAsync();
		}
	}
}

public class ThumbnailUploadResult
{
	public string public_id;
	public string secure_url;
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
	public string thumbnailPublicId { get; set; }

	[FirestoreProperty]
	public SceneData sceneData { get; set; }

	[FirestoreProperty]
	public bool isDeleted { get; set; }
	[FirestoreProperty]
	public Timestamp deletedAt { get; set; }
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
