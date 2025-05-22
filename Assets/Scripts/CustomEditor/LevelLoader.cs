using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public class LevelLoader : MonoBehaviour
{
	[SerializeField] private GameObject[] permanentObjects;


	// Start is called before the first frame update
	void Start()
    {
		Load(LevelDataTransfer.SceneDataToLoad);
    }

    // Update is called once per frame
    void Update()
    {

    }

	public void Load(SceneData selectedSceneData = null)
	{
		SceneData sceneData;

		if (selectedSceneData == null)
		{
			if (!File.Exists(SaveSystem.saveFilePath + "SceneData.json")) return;

			string sceneDataString = File.ReadAllText(SaveSystem.saveFilePath + "SceneData.json");
			sceneData = JsonUtility.FromJson<SceneData>(sceneDataString);
		}
		else
		{
			sceneData = selectedSceneData;
		}

		UnityEngine.Object[] prefabObjectSOs = Resources.LoadAll("SceneObjects", typeof(SceneObjectSO));

		Dictionary<GameObject, ModifiableObjectData> loadedObjects = new Dictionary<GameObject, ModifiableObjectData>();

		int j = 0;
		foreach (var permanentData in sceneData.permanentObjectsInScene)
		{
			if(permanentObjects[j].transform.GetComponent<TeleportationZone>() != null)
				permanentObjects[j].transform.position = permanentData.position + Vector3.up;
			else
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

			if (objectPrefabSO != null)
			{
				GameObject sceneObject = Instantiate(objectPrefabSO.workingObjectPrefab, objectData.position, Quaternion.identity); /*objectData.scale*/
				sceneObject.transform.eulerAngles = objectData.rotation;
				sceneObject.transform.localScale = objectData.scale;

				loadedObjects.Add(sceneObject, objectData);


				if (sceneObject.GetComponent<MaterialDispenser>())
				{
					MaterialDispenser dispenser = sceneObject.GetComponent<MaterialDispenser>();
					dispenser.SetupDispenser((Materials)objectData.materialType, objectData.materialNumber);
				}

				if (sceneObject.GetComponent<Activable>())
					sceneObject.GetComponent<Activable>().needPower = true;
			}
		}

		foreach (var lo in loadedObjects)
		{
			ModifiableObjectData currentDataSearched = lo.Value;
			if (currentDataSearched.activablesId.Count > 0)
			{
				lo.Key.GetComponent<Activator>().linkedActivable.Clear();
				for (int i = 0; i < currentDataSearched.activablesId.Count; i++)
				{
					lo.Key.GetComponent<Activator>().linkedActivable.Add(GetObjectById(loadedObjects, currentDataSearched.activablesId[i]).GetComponent<Activable>());
				}
			}
		}

		GameObject GetObjectById(Dictionary<GameObject, ModifiableObjectData> dictionnary, string objectId)
		{
			foreach (var pair in dictionnary)
			{
				if (EqualityComparer<string>.Default.Equals(pair.Value.objectId, objectId))
				{
					return pair.Key;
				}
			}

			return null;
		}

		//cameraHolder.position = sceneData.camPosition;
		//cameraHolder.eulerAngles = new Vector3(0, sceneData.camRotation.y, 0);
		//cameraMain.localEulerAngles = new Vector3(sceneData.camRotation.x, 0, 0);
	}
}
