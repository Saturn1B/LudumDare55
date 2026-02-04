using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Firebase.Auth;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class FirestoreManager : MonoBehaviour
{
	private FirebaseFirestore firestore;

	private void Awake()
	{
		firestore = FirebaseFirestore.DefaultInstance;
	}

	public async Task UploadLevel(SceneData sceneData)
	{
		if (string.IsNullOrEmpty(sceneData.uploadId))
			await FirstUpload(sceneData);
		else
			await UpdateUpload(sceneData);
	}

	private async Task FirstUpload(SceneData sceneData)
	{
		string userId = FirebaseAuth.DefaultInstance.CurrentUser.UserId;

		DocumentReference levelRef = firestore.Collection("levels").Document();

		string uploadId = levelRef.Id;

		sceneData.uploadId = uploadId;

		LevelData levelData = new LevelData
		{
			uploadId = uploadId,
			levelName = sceneData.levelName,
			authorId = userId,
			authorName = null,
			createdAt = Timestamp.GetCurrentTimestamp(),
			updatedAt = Timestamp.GetCurrentTimestamp(),
			likesCount = 0,
			thumbnailPath = $"thumbnails/{uploadId}.png",
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

	private async Task UpdateUpload(SceneData sceneData)
	{
		DocumentReference levelRef = firestore.Collection("levels").Document(sceneData.uploadId);

		await levelRef.UpdateAsync(new Dictionary<string, object>
		{
			{"sceneData", sceneData },
			{"updatedAt", Timestamp.GetCurrentTimestamp() }
		});
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
