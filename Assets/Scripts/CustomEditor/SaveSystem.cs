using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Newtonsoft.Json;

public class SaveSystem : MonoBehaviour
{
	[SerializeField] Canvas editorCanvas;

	public List<ModifiableObject> objectInScene = new List<ModifiableObject>();

	[SerializeField] private Transform cameraHolder, cameraMain;

	[SerializeField] private Undeletable[] permanentObjects;

	[SerializeField] private Camera captureCamera;

	public static SaveSystem Instance { get; private set; }

	public static string saveFilePath;

	public string currentUploadId;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}

		GenerateSaveFilePath();
	}

	private void Start()
	{
		if (LevelDataTransfer.SceneDataToTest != null)
			Load(LevelDataTransfer.SceneDataToTest);
		else if (LevelDataTransfer.SceneDataToLoad != null)
			Load(LevelDataTransfer.SceneDataToLoad);
		else
			SaveOnDisk();
	}

	public static void GenerateSaveFilePath()
	{
		if (string.IsNullOrEmpty(saveFilePath))
			saveFilePath = Application.persistentDataPath + "/EditorLevelsData/";
	}

	[ContextMenu("Upload")]
	public async void UploadData()
	{
		SceneData sceneData = SaveSceneData();

		Texture2D thumbnail = Capture();

		await FindObjectOfType<FirestoreManager>().UploadLevel(sceneData, thumbnail);
		SaveOnDisk(sceneData);
	}

	[ContextMenu("Save")]
	public SceneData SaveSceneData()
	{
		SceneData sceneData = new SceneData();
		sceneData.objectsInScene = new ModifiableObjectData[objectInScene.Count];

		int i = 0;
		foreach (var modifiable in objectInScene)
		{
			ModifiableObjectData objectData = SaveBaseModifiableObjectData(modifiable);
			//ModifiableObjectData objectData = new ModifiableObjectData();

			//objectData.objectId = modifiable.objectId;
			//objectData.objectName = modifiable.gameObject.name;

			//objectData.position = modifiable.transform.position;
			//objectData.rotation = modifiable.transform.eulerAngles;
			//objectData.scale = modifiable.transform.localScale;

			//objectData.canDelete = modifiable.GetComponent<Undeletable>() == null ? false : true;

			if (modifiable.hasChildObjects)
			{
				objectData.childObjects = new ModifiableObjectData[modifiable.childObjects.Length];

				for (int k = 0; k < objectData.childObjects.Length; k++)
				{
					objectData.childObjects[k] = SaveBaseModifiableObjectData(modifiable.childObjects[k]);
				}
			}

			if (objectData.activablesId == null)
				objectData.activablesId = new List<string>();
			if (objectData.activatorsId == null)
				objectData.activatorsId = new List<string>();

			if (modifiable.GetComponent<ActivatorEditor>() != null)
			{
				ActivatorEditor activator = modifiable.GetComponent<ActivatorEditor>();
				foreach (var item in activator.activables)
				{
					objectData.activablesId.Add(item.GetComponent<ModifiableObject>().objectId);
				}
			}
			else if (modifiable.GetComponent<ActivableEditor>() != null)
			{
				ActivableEditor activable = modifiable.GetComponent<ActivableEditor>();
				foreach (var item in activable.activators)
				{
					objectData.activatorsId.Add(item.GetComponent<ModifiableObject>().objectId);
				}
			}
			else if (modifiable.GetComponent<DispenserEditor>() != null)
			{
				DispenserEditor dispenser = modifiable.GetComponent<DispenserEditor>();
				objectData.materialType = (int)dispenser.materialType;
				objectData.materialNumber = dispenser.materialNumber;
			}

			sceneData.objectsInScene[i] = objectData;

			i++;
		}

		sceneData.permanentObjectsInScene = new PermanentObjectData[permanentObjects.Length];

		int j = 0;
		foreach (Undeletable permanentObj in permanentObjects)
		{
			PermanentObjectData permanentData = new PermanentObjectData();
			permanentData.position = permanentObj.transform.position;
			permanentData.rotation = permanentObj.transform.eulerAngles;
			permanentData.scale = permanentObj.transform.localScale;

			sceneData.permanentObjectsInScene[j] = permanentData;

			j++;
		}

		sceneData.camPosition = cameraHolder.position;
		sceneData.camRotation = new Vector3(cameraMain.localRotation.eulerAngles.x, cameraHolder.rotation.eulerAngles.y, 0);

		string saveFileName;
		string saveFileId;

		if (string.IsNullOrEmpty(LevelDataTransfer.levelName))
			saveFileName = "unknownSave-" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
		else
			saveFileName = LevelDataTransfer.levelName;

		saveFileId = (DateTime.Now.Millisecond * DateTime.Now.Second).ToString();

		sceneData.levelName = saveFileName;
		sceneData.levelId = saveFileId;

		sceneData.uploadId = currentUploadId;

		return sceneData;
	}

	public void SaveOnDisk(SceneData customSceneData = null)
	{
		SceneData sceneData;

		if (customSceneData != null)
			sceneData = customSceneData;
		else
			sceneData = SaveSceneData();

		string sceneDataString = JsonConvert.SerializeObject(sceneData, Formatting.Indented, new JsonSerializerSettings
		{
			ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
			Converters = new JsonConverter[] { new Vector3Converter() }
		});

		string saveFileNameId = sceneData.levelName + sceneData.levelId;

		if (!Directory.Exists(saveFilePath))
			Directory.CreateDirectory(saveFilePath);

		if (LevelDataTransfer.SceneDataToLoad != null)
		{
			File.Delete(SaveSystem.saveFilePath + LevelDataTransfer.SceneDataToLoad.levelName + LevelDataTransfer.SceneDataToLoad.levelId + ".json");
			File.Delete(SaveSystem.saveFilePath + LevelDataTransfer.SceneDataToLoad.levelName + LevelDataTransfer.SceneDataToLoad.levelId + ".png");
			LevelDataTransfer.SceneDataToLoad = sceneData;
		}

		System.IO.File.WriteAllText(saveFilePath + $"{saveFileNameId}.json", sceneDataString);
		StartCoroutine(CaptureScreen(saveFileNameId));
	}

	public void Load(SceneData selectedSceneData = null)
	{
		SceneData sceneData;

		if (selectedSceneData == null)
		{
			if (!File.Exists(saveFilePath + "SceneData.json")) return;

			string sceneDataString = File.ReadAllText(saveFilePath + "SceneData.json");
			sceneData = JsonConvert.DeserializeObject<SceneData>(sceneDataString);
		}
		else
		{
			sceneData = selectedSceneData;
		}

		UnityEngine.Object[] prefabObjectSOs = Resources.LoadAll("SceneObjects", typeof(SceneObjectSO));

		Dictionary<ModifiableObject, string> loadedObjects = new Dictionary<ModifiableObject, string>();

		int j = 0;
		foreach (var permanentData in sceneData.permanentObjectsInScene)
		{
			permanentObjects[j].transform.position = permanentData.position;
			permanentObjects[j].transform.eulerAngles = permanentData.rotation;
			permanentObjects[j].transform.localScale = permanentData.scale;

			j++;
		}

		foreach (var objectData in sceneData.objectsInScene)
		{
			SceneObjectSO objectPrefabSO = null;
			foreach (var sObject in prefabObjectSOs)
			{
				SceneObjectSO currentObjectSO = (SceneObjectSO)sObject;
				if (objectData.objectName == currentObjectSO.objectName)
				{
					objectPrefabSO = currentObjectSO;
				}
			}

			if(objectPrefabSO != null)
			{
				ModifiableObject modifiable = ObjectPlacer.Instance.CreateObject(objectData.position, objectData.rotation, objectData.scale, objectPrefabSO.objectPrefab, objectPrefabSO.objectName);
				modifiable.OverrideID(objectData.objectId);
				loadedObjects.Add(modifiable, modifiable.objectId);

				if (objectData.childObjects != null && objectData.childObjects.Length > 0)
				{
					for (int i = 0; i < modifiable.childObjects.Length; i++)
					{
						modifiable.childObjects[i].transform.position = objectData.childObjects[i].position;
						modifiable.childObjects[i].transform.eulerAngles = objectData.childObjects[i].rotation;
						modifiable.childObjects[i].transform.localScale = objectData.childObjects[i].scale;
						modifiable.childObjects[i].transform.name = objectData.objectName;
						modifiable.childObjects[i].OverrideID(objectData.objectId);
					}
				}

				if(modifiable.GetComponent<DispenserEditor>() != null)
				{
					DispenserEditor dispenser = modifiable.GetComponent<DispenserEditor>();
					dispenser.SetMaterial(objectData.materialType, false);
					dispenser.SetNumber(objectData.materialNumber);
				}
			}
		}

		foreach (var lo in loadedObjects)
		{
			ModifiableObjectData currentDataSearched = GetObjectDataById(lo.Value, sceneData);
			if (currentDataSearched.activablesId.Count > 0)
			{
				for (int i = 0; i < currentDataSearched.activablesId.Count; i++)
				{
					lo.Key.GetComponent<ActivatorEditor>().activables.Add(GetObjectById(loadedObjects, currentDataSearched.activablesId[i]).GetComponent<ActivableEditor>());
				}
			}
			if (currentDataSearched.activatorsId.Count > 0)
			{
				for (int i = 0; i < currentDataSearched.activatorsId.Count; i++)
				{
					lo.Key.GetComponent<ActivableEditor>().activators.Add(GetObjectById(loadedObjects, currentDataSearched.activatorsId[i]).GetComponent<ActivatorEditor>());
				}
			}
		}

		cameraHolder.position = sceneData.camPosition;
		cameraHolder.eulerAngles = new Vector3(0, sceneData.camRotation.y, 0);
		cameraMain.localEulerAngles = new Vector3(sceneData.camRotation.x, 0, 0);

		currentUploadId = sceneData.uploadId;
	}

	ModifiableObjectData GetObjectDataById(string objectId, SceneData sceneData)
	{
		foreach (var objectData in sceneData.objectsInScene)
		{
			if(objectData.objectId == objectId)
			{
				return objectData;
			}
		}

		return null;
	}

	ModifiableObject GetObjectById(Dictionary<ModifiableObject, string> dictionnary, string objectId)
	{
		foreach (var pair in dictionnary)
		{
			if(EqualityComparer<string>.Default.Equals(pair.Value, objectId))
			{
				return pair.Key;
			}
		}

		return null;
	}

	private IEnumerator CaptureScreen(string saveFileNameId)
	{
		yield return null;
		editorCanvas.enabled = false;

		yield return new WaitForEndOfFrame();

		ScreenCapture.CaptureScreenshot(saveFilePath + $"{saveFileNameId}.png");

		editorCanvas.enabled = true;
	}

	private Texture2D Capture()
	{
		RenderTexture rt = new RenderTexture(Screen.height, Screen.width, 24);
		Texture2D texture = new Texture2D(Screen.height, Screen.width, TextureFormat.RGB24, false);

		captureCamera.targetTexture = rt;
		RenderTexture.active = rt;

		captureCamera.Render();

		texture.ReadPixels(new Rect(0, 0, Screen.height, Screen.width), 0, 0);
		texture.Apply();

		captureCamera.targetTexture = null;
		RenderTexture.active = null;

		Destroy(rt);

		return texture;
	}

	private ModifiableObjectData SaveBaseModifiableObjectData(ModifiableObject modifiable)
	{
		ModifiableObjectData objectData = new ModifiableObjectData();

		objectData.objectId = modifiable.objectId;
		objectData.objectName = modifiable.gameObject.name;

		objectData.position = modifiable.transform.position;
		objectData.rotation = modifiable.transform.eulerAngles;
		objectData.scale = modifiable.transform.localScale;

		objectData.canDelete = modifiable.GetComponent<Undeletable>() == null ? false : true;

		return objectData;
	}
}

[System.Serializable][FirestoreData]
public class SceneData
{
	[FirestoreProperty]
	public string levelName { get; set; }
	[FirestoreProperty]
	public string levelId { get; set; }
	public Vector3 camPosition { get; set; }
	[JsonIgnore][FirestoreProperty("camPosition")]
	private float[] camPositionSerialized
	{
		get => new float[] { camPosition.x, camPosition.y, camPosition.z };
		set => camPosition = new Vector3(value[0], value[1], value[2]);
	}
	public Vector3 camRotation { get; set; }
	[JsonIgnore][FirestoreProperty("camRotation")]
	private float[] camRotationSerialized
	{
		get => new float[] { camRotation.x, camRotation.y, camRotation.z };
		set => camRotation = new Vector3(value[0], value[1], value[2]);
	}
	[FirestoreProperty]
	public ModifiableObjectData[] objectsInScene { get; set; }
	[FirestoreProperty]
	public PermanentObjectData[] permanentObjectsInScene { get; set; }
	[FirestoreProperty]
	public string uploadId { get; set; }
}

[System.Serializable][FirestoreData]
public class ModifiableObjectData
{
	[FirestoreProperty]
	public string objectId { get; set; }
	[FirestoreProperty]
	public string objectName { get; set; }
	public Vector3 position { get; set; }
	[JsonIgnore][FirestoreProperty("position")]
	private float[] positionSerialized
	{
		get => new float[] { position.x, position.y, position.z };
		set => position = new Vector3(value[0], value[1], value[2]);
	}
	public Vector3 rotation { get; set; }
	[JsonIgnore][FirestoreProperty("rotation")]
	private float[] rotationSerialized
	{
		get => new float[] { rotation.x, rotation.y, rotation.z };
		set => rotation = new Vector3(value[0], value[1], value[2]);
	}
	public Vector3 scale { get; set; }
	[JsonIgnore][FirestoreProperty("scale")]
	private float[] scaleSerialized
	{
		get => new float[] { scale.x, scale.y, scale.z };
		set => scale = new Vector3(value[0], value[1], value[2]);
	}
	[FirestoreProperty]
	public bool canDelete { get; set; }
	[FirestoreProperty]
	public List<string> activatorsId { get; set; }
	[FirestoreProperty]
	public List<string> activablesId { get; set; }
	[FirestoreProperty]
	public int materialType { get; set; }
	[FirestoreProperty]
	public int materialNumber { get; set; }
	[FirestoreProperty]
	public ModifiableObjectData[] childObjects { get; set; }
}

[System.Serializable][FirestoreData]
public class PermanentObjectData
{
	public Vector3 position { get; set; }
	[JsonIgnore][FirestoreProperty("position")]
	private float[] positionSerialized
	{
		get => new float[] { position.x, position.y, position.z };
		set => position = new Vector3(value[0], value[1], value[2]);
	}
	public Vector3 rotation { get; set; }
	[JsonIgnore][FirestoreProperty("rotation")]
	private float[] rotationSerialized
	{
		get => new float[] { rotation.x, rotation.y, rotation.z };
		set => rotation = new Vector3(value[0], value[1], value[2]);
	}
	public Vector3 scale { get; set; }
	[JsonIgnore][FirestoreProperty("scale")]
	private float[] scaleSerialized
	{
		get => new float[] { scale.x, scale.y, scale.z };
		set => scale = new Vector3(value[0], value[1], value[2]);
	}
}

public class Vector3Converter : JsonConverter<Vector3>
{
	public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("x");
		writer.WriteValue(value.x);
		writer.WritePropertyName("y");
		writer.WriteValue(value.y);
		writer.WritePropertyName("z");
		writer.WriteValue(value.z);
		writer.WriteEndObject();
	}

	public override Vector3 ReadJson(JsonReader reader, System.Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		float x = 0, y = 0, z = 0;

		while (reader.Read())
		{
			if (reader.TokenType == JsonToken.PropertyName)
			{
				string propertyName = (string)reader.Value;
				if (!reader.Read()) continue;

				switch (propertyName)
				{
					case "x": x = (float)(double)reader.Value; break;
					case "y": y = (float)(double)reader.Value; break;
					case "z": z = (float)(double)reader.Value; break;
				}
			}
			else if (reader.TokenType == JsonToken.EndObject)
			{
				break;
			}
		}

		return new Vector3(x, y, z);
	}
}
