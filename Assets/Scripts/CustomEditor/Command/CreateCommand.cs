using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateCommand : ICommand<GameObject>
{
	private Vector3 summonPos;
	private Quaternion summonRot;
	private Vector3 summonScale;
	private GameObject objectPrefab;
	private string objectName;
	private string currentObjectName;

	private GameObject createdObject = null;
	private string createdObjectId = null;

	public CreateCommand(Vector3 summonPos, Quaternion summonRot, Vector3 summonScale, GameObject objectPrefab, string objectName, string currentObjectName)
	{
		this.summonPos = summonPos;
		this.summonRot = summonRot;
		this.summonScale = summonScale;
		this.objectPrefab = objectPrefab;
		this.objectName = objectName;
		this.currentObjectName = currentObjectName;
	}

	public GameObject Execute(bool isRedo)
	{
		if (isRedo)
		{
			ObjectSelection.Instance.DeselectObject();
			EditorHUDManager.Instance.CloseDispenserEditor();
			EditorHUDManager.Instance.CloseInteractionEditor();
		}

		GameObject go = GameObject.Instantiate(objectPrefab, summonPos, Quaternion.identity);

		go.transform.rotation = summonRot;
		go.transform.localScale = summonScale;

		go.name = currentObjectName;

		go.GetComponent<ModifiableObject>().parentPrefab = objectPrefab;

		SaveSystem.Instance.objectInScene.Add(go.GetComponent<ModifiableObject>());

		if (go.GetComponent<ActivatorEditor>())
		{
			go.GetComponent<ActivatorEditor>().activatorName = objectName;
		}
		if (go.GetComponent<ActivableEditor>())
		{
			go.GetComponent<ActivableEditor>().activableName = objectName;
		}

		if (!string.IsNullOrEmpty(createdObjectId))
		{
			go.GetComponent<ModifiableObject>().OverrideID(createdObjectId);
		}

		createdObject = go;
		createdObjectId = createdObject.GetComponent<ModifiableObject>().objectId;

		return go;
	}

	void ICommandBase.Execute(bool isRedo) => Execute(isRedo);

	public void Undo()
	{
		CheckTarget();

		ObjectSelection.Instance.DeselectObject();
		EditorHUDManager.Instance.CloseDispenserEditor();
		EditorHUDManager.Instance.CloseInteractionEditor();

		SaveSystem.Instance.objectInScene.Remove(createdObject.transform.GetComponent<ModifiableObject>());
		GameObject.Destroy(createdObject);
	}

	private void CheckTarget()
	{
		if (createdObject == null && !string.IsNullOrEmpty(createdObjectId))
		{
			foreach (var obj in SaveSystem.Instance.objectInScene)
			{
				if (obj.objectId == createdObjectId)
				{
					createdObject = obj.gameObject;
					return;
				}
			}
		}
	}
}
