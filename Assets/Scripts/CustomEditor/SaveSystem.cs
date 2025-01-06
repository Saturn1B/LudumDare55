using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveSystem : MonoBehaviour
{
	[HideInInspector] public List<ModifiableObject> objectInScene = new List<ModifiableObject>();

	public static SaveSystem Instance { get; private set; }

	string saveFilePath;

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

		saveFilePath = Application.persistentDataPath + "/SceneData.json";
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

			sceneData.objectsInScene[i] = objectData;

			i++;
		}

		string sceneDataString = JsonUtility.ToJson(sceneData);
		System.IO.File.WriteAllText(saveFilePath, sceneDataString);
	}

	[ContextMenu("Load")]
	public void Load()
	{
		if (!File.Exists(saveFilePath)) return;

		string sceneDataString = File.ReadAllText(saveFilePath);
		SceneData sceneData = JsonUtility.FromJson<SceneData>(sceneDataString);

		UnityEngine.Object[] prefabObjectSOs = Resources.LoadAll("SceneObjects", typeof(SceneObjectSO));

		Dictionary<ModifiableObject, string> loadedObjects = new Dictionary<ModifiableObject, string>();

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
}

[System.Serializable]
public class SceneData
{
	public ModifiableObjectData[] objectsInScene;
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
}
