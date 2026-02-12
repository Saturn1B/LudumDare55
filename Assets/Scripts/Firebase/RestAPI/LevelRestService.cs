using UnityEngine;
using Proyecto26;
using Firebase.Auth;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Generic;
using System;

public class LevelRestService : MonoBehaviour
{
	public static LevelRestService Instance;

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
			{ "Authorization", "Bearer " + token }
		};
	}

	public async Task UploadLevel(SceneData sceneData)
	{
		var user = FirebaseAuth.DefaultInstance.CurrentUser;
		if (user == null)
		{
			Debug.LogError("No authenticated user.");
			return;
		}

		string token = await user.TokenAsync(true);

		if (string.IsNullOrEmpty(sceneData.uploadId))
			sceneData.uploadId = Guid.NewGuid().ToString();

		string uploadId = sceneData.uploadId;

		string postUrl = $"{baseUrl}/levels?documentId={uploadId}";

		var body = new
		{
			fields = new
			{
				uploadId = new { stringValue = uploadId },
				levelName = new { stringValue = sceneData.levelName ?? "" },
				authorId = new { stringValue = user.UserId },
				authorName = new { stringValue = "" },
				createdAt = new { timestampValue = DateTime.UtcNow.ToString("o") },
				updatedAt = new { timestampValue = DateTime.UtcNow.ToString("o") },
				likesCount = new { integerValue = "0" },
				thumbnailPath = new { stringValue = "" },
				thumbnailPublicId = new { stringValue = "" },
				isDeleted = new { booleanValue = false },
				sceneData = SceneDataToFirestore(sceneData)
			}
		};

		await RestClient.Post(new RequestHelper
		{
			Uri = postUrl,
			Headers = AuthHeader(token),
			BodyString = JsonConvert.SerializeObject(body),
			ContentType = "application/json"
		}).AsTask();

		Debug.Log("Level uploaded succesfully");
	}

	// HELPER FUNCTION

	private object Vector3ToFirestore(Vector3 v)
	{
		return new
		{
			mapValue = new
			{
				fields = new
				{
					x = new { doubleValue = v.x },
					y = new { doubleValue = v.y },
					z = new { doubleValue = v.z },
				}
			}
		};
	}

	private object SceneDataToFirestore(SceneData data)
	{
		return new
		{
			mapValue = new
			{
				fields = new
				{
					levelName = new { stringValue = data.levelName ?? "" },
					levelId = new { stringValue = data.levelId ?? "" },
					uploadId = new { stringValue = data.uploadId ?? "" },
					camPosition = Vector3ToFirestore(data.camPosition),
					camRotation = Vector3ToFirestore(data.camRotation),
					objectsInScene = ModifiableArrayToFirestore(data.objectsInScene),
					permanentObjectsInScene = PermanentArrayToFirestore(data.permanentObjectsInScene)
				}
			}
		};
	}

	private object ModifiableArrayToFirestore(ModifiableObjectData[] array)
	{
		if (array == null || array.Length == 0)
		{
			return new { arrayValue = new { values = new object[] { } } };
		}

		var values = new List<object>();

		foreach (var obj in array)
		{
			values.Add(ModifiableObjectToFirestore(obj));
		}

		return new { arrayValue = new { values } };
	}

	private object ModifiableObjectToFirestore(ModifiableObjectData obj)
	{
		return new
		{
			mapValue = new
			{
				fields = new
				{
					objectId = new { stringValue = obj.objectId ?? "" },
					objectName = new { stringValue = obj.objectName ?? "" },
					position = Vector3ToFirestore(obj.position),
					rotation = Vector3ToFirestore(obj.rotation),
					scale = Vector3ToFirestore(obj.scale),
					canDelete = new { booleanValue = obj.canDelete },
					materialType = new { integerValue = obj.materialType.ToString() },
					materialNumber = new { integerValue = obj.materialNumber.ToString() },
					activatorsId = StringListToFirestore(obj.activatorsId),
					activablesId = StringListToFirestore(obj.activablesId),
					childObjects = ModifiableArrayToFirestore(obj.childObjects)
				}
			}
		};
	}


	private object StringListToFirestore(List<string> list)
	{
		if (list == null || list.Count == 0)
			return new { arrayValue = new { values = new object[] { } } };

		var values = new List<object>();

		foreach (var s in list)
		{
			values.Add(new { stringValue = s });
		}

		return new { arrayValue = new { values } };
	}

	private object PermanentArrayToFirestore(PermanentObjectData[] array)
	{
		if (array == null || array.Length == 0)
		{
			return new { arrayValue = new { values = new object[] { } } };
		}

		var values = new List<object>();

		foreach (var obj in array)
		{
			values.Add(new
			{
				mapValue = new
				{
					fields = new
					{
						position = Vector3ToFirestore(obj.position),
						rotation = Vector3ToFirestore(obj.rotation),
						scale = Vector3ToFirestore(obj.scale)
					}
				}
			});
		}

		return new { arrayValue = new { values } };
	}
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
