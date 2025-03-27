using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
	[SerializeField] Canvas editorCanvas;

	public List<ModifiableObject> objectInScene = new List<ModifiableObject>();

	[SerializeField] private Transform cameraHolder, cameraMain;

	[SerializeField] private Undeletable[] permanentObjects;

	public static SaveSystem Instance { get; private set; }

	public static string saveFilePath;

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
		if (LevelDataTransfer.SceneDataToLoad != null)
			Load(LevelDataTransfer.SceneDataToLoad);
	}

	public static void GenerateSaveFilePath()
	{
		if (string.IsNullOrEmpty(saveFilePath))
			saveFilePath = Application.persistentDataPath + "/EditorLevelsData/";
	}

	[ContextMenu("Save")]
	public void Save()
	{
		SceneData sceneData = new SceneData();
		sceneData.objectsInScene = new ModifiableObjectData[objectInScene.Count];

		int i = 0;
		foreach (var modifiable in objectInScene)
		{
			ModifiableObjectData objectData = new ModifiableObjectData();

			objectData.objectId = modifiable.objectId;
			objectData.objectName = modifiable.gameObject.name;

			objectData.position = modifiable.transform.position;
			objectData.rotation = modifiable.transform.eulerAngles;
			objectData.scale = modifiable.transform.localScale;

			objectData.canDelete = modifiable.GetComponent<Undeletable>() == null ? false : true;

			if(modifiable.GetComponent<ActivatorEditor>() != null)
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

		string sceneDataString = JsonUtility.ToJson(sceneData);

		string saveFileNameId = saveFileName + saveFileId;

		if (!Directory.Exists(saveFilePath))
			Directory.CreateDirectory(saveFilePath);

		if(LevelDataTransfer.SceneDataToLoad != null)
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
			sceneData = JsonUtility.FromJson<SceneData>(sceneDataString);
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
}

[System.Serializable]
public class SceneData
{
	public string levelName;
	public string levelId;
	public Vector3 camPosition;
	public Vector3 camRotation;
	public ModifiableObjectData[] objectsInScene;
	public PermanentObjectData[] permanentObjectsInScene;
}

[System.Serializable]
public class ModifiableObjectData
{
	public string objectId;
	public string objectName;
	public Vector3 position;
	public Vector3 rotation;
	public Vector3 scale;
	public bool canDelete;
	public List<string> activatorsId = new List<string>();
	public List<string> activablesId = new List<string>();
	public int materialType;
	public int materialNumber;
}

[System.Serializable]
public class PermanentObjectData
{
	public Vector3 position;
	public Vector3 rotation;
	public Vector3 scale;
}
