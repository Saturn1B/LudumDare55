using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteCommand : ICommand<GameObject>
{
	public string actionDescription => $"Delete {objectName}";

	private GameObject target;
	private string targetId;

	private GameObject prefabObject;

	private Vector3 objectPosition;
	private Quaternion objectRotation;
	private Vector3 objectScale;
	private string objectName;

	private DispenserSave dispenserSave;
	private ActivatorSave activatorSave;
	private ActivableSave activableSave;

	public DeleteCommand(GameObject target, string targetId, GameObject prefabObject, DispenserSave dispenserSave)
	{
		this.target = target;
		this.targetId = targetId;
		this.dispenserSave = dispenserSave;

		this.prefabObject = prefabObject;

		objectName = target.name;

		objectPosition = target.transform.position;
		objectRotation = target.transform.rotation;
		objectScale = target.transform.localScale;
	}
	public DeleteCommand(GameObject target, string targetId, GameObject prefabObject, ActivatorSave activatorSave)
	{
		this.target = target;
		this.targetId = targetId;
		this.activatorSave = activatorSave;

		this.prefabObject = prefabObject;

		objectName = target.name;

		objectPosition = target.transform.position;
		objectRotation = target.transform.rotation;
		objectScale = target.transform.localScale;
	}
	public DeleteCommand(GameObject target, string targetId, GameObject prefabObject, ActivableSave activableSave)
	{
		this.target = target;
		this.targetId = targetId;
		this.activableSave = activableSave;

		this.prefabObject = prefabObject;

		objectName = target.name;

		objectPosition = target.transform.position;
		objectRotation = target.transform.rotation;
		objectScale = target.transform.localScale;
	}
	public DeleteCommand(GameObject target, string targetId, GameObject prefabObject)
	{
		this.target = target;
		this.targetId = targetId;

		this.prefabObject = prefabObject;

		objectName = target.name;

		objectPosition = target.transform.position;
		objectRotation = target.transform.rotation;
		objectScale = target.transform.localScale;
	}

	public GameObject Execute(bool isRedo)
	{
		CheckTarget();

		if (isRedo)
		{
			ObjectSelection.Instance.DeselectObject();
			EditorHUDManager.Instance.CloseDispenserEditor();
			EditorHUDManager.Instance.CloseInteractionEditor();
		}

		if (target.transform.GetComponent<ActivableEditor>())
		{
			ActivableEditor currentActivable = target.transform.GetComponent<ActivableEditor>();
			foreach (var activator in currentActivable.activators)
			{
				if(activator.activables.Count <= 1)
					activator.SwitchWarningSignState(true);
			}
			currentActivable.RemoveFromActivator();
		}
		if (target.transform.GetComponent<ActivatorEditor>())
		{
			ActivatorEditor currentActivator = target.transform.GetComponent<ActivatorEditor>();
			foreach (var activable in currentActivator.activables)
			{
				if (activable.activators.Count <= 1)
					activable.SwitchWarningSignState(true);
			}
			currentActivator.RemoveFromActivable();
		}
		SaveSystem.Instance.objectInScene.Remove(target.transform.GetComponent<ModifiableObject>());
		GameObject.Destroy(target);

		return prefabObject;
	}

	void ICommandBase.Execute(bool isRedo) => Execute(isRedo);

	public void Undo()
	{
		//CheckTarget();

		ObjectSelection.Instance.DeselectObject();
		EditorHUDManager.Instance.CloseDispenserEditor();
		EditorHUDManager.Instance.CloseInteractionEditor();

		GameObject go = GameObject.Instantiate(prefabObject, objectPosition, objectRotation);

		go.transform.localScale = objectScale;

		go.name = objectName;

		go.GetComponent<ModifiableObject>().parentPrefab = prefabObject;

		SaveSystem.Instance.objectInScene.Add(go.GetComponent<ModifiableObject>());

		if (go.GetComponent<ActivatorEditor>())
		{
			ActivatorEditor currentActivatorEditor = go.GetComponent<ActivatorEditor>();
			currentActivatorEditor.activatorName = objectName;
			currentActivatorEditor.activables = new List<ActivableEditor>();
			for (int i = 0; i < activatorSave.activableIds.Count; i++)
			{
				foreach (var obj in SaveSystem.Instance.objectInScene)
				{
					if (obj.objectId == activatorSave.activableIds[i])
					{
						currentActivatorEditor.activables.Add(obj.transform.GetComponent<ActivableEditor>());
						obj.transform.GetComponent<ActivableEditor>().activators.Add(currentActivatorEditor);
						break;
					}
				}
			}
			foreach (var activable in currentActivatorEditor.activables)
			{
				activable.SwitchWarningSignState(false);
			}
		}
		if (go.GetComponent<ActivableEditor>())
		{
			ActivableEditor currentActivableEditor = go.GetComponent<ActivableEditor>();
			currentActivableEditor.activableName = objectName;
			currentActivableEditor.activators = new List<ActivatorEditor>();
			for (int i = 0; i < activableSave.activatorIds.Count; i++)
			{
				foreach (var obj in SaveSystem.Instance.objectInScene)
				{
					if (obj.objectId == activableSave.activatorIds[i])
					{
						currentActivableEditor.activators.Add(obj.transform.GetComponent<ActivatorEditor>());
						obj.transform.GetComponent<ActivatorEditor>().activables.Add(currentActivableEditor);
						break;
					}
				}
			}
			foreach (var activator in currentActivableEditor.activators)
			{
				activator.SwitchWarningSignState(false);
			}
		}
		if (go.GetComponent<DispenserEditor>())
		{
			DispenserEditor currentDispenserEditor = go.GetComponent<DispenserEditor>();
			currentDispenserEditor.SetMaterial((int)dispenserSave.materialType, false);
			currentDispenserEditor.SetNumber(dispenserSave.materialNumber);
		}

		go.GetComponent<ModifiableObject>().OverrideID(targetId);
	}

	private void CheckTarget()
	{
		if (target == null && !string.IsNullOrEmpty(targetId))
		{
			foreach (var obj in SaveSystem.Instance.objectInScene)
			{
				if (obj.objectId == targetId)
				{
					target = obj.gameObject;
					return;
				}
			}
		}
	}
}

[System.Serializable]
public class DispenserSave
{
	public Materials materialType;
	public int materialNumber;

	public DispenserSave(Materials materialType, int materialNumber)
	{
		this.materialType = materialType;
		this.materialNumber = materialNumber;
	}
}

[System.Serializable]
public class ActivatorSave
{
	public List<string> activableIds;

	public ActivatorSave(List<ActivableEditor> activables)
	{
		activableIds = new List<string>();
		for (int i = 0; i < activables.Count; i++)
		{
			activableIds.Add(activables[i].transform.GetComponent<ModifiableObject>().objectId);
		}
	}
}

[System.Serializable]
public class ActivableSave
{
	public List<string> activatorIds;

	public ActivableSave(List<ActivatorEditor> activators)
	{
		activatorIds = new List<string>();
		for (int i = 0; i < activators.Count; i++)
		{
			activatorIds.Add(activators[i].transform.GetComponent<ModifiableObject>().objectId);
		}
	}
}
