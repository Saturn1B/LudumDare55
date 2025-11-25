using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleCommand : ICommand<GameObject>
{
	public string actionDescription => $"{actionType} {target.name}";
	private string actionType = "Scale";

	private GameObject target;
	private string targetId;
	private Vector3 beforeScale;
	private Vector3 afterScale;
	private Vector3 beforePosition;
	private Vector3 afterPosition;

	public ScaleCommand(GameObject target, string targetId, Vector3 beforeScale, Vector3 afterScale, Vector3 beforePosition, Vector3 afterPosition, bool isCubeRotated = false)
	{
		this.target = target;
		this.targetId = targetId;
		this.beforeScale = beforeScale;
		this.afterScale = afterScale;
		this.beforePosition = beforePosition;
		this.afterPosition = afterPosition;

		actionType = isCubeRotated ? "Rotate" : "Scale";
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

		target.transform.localScale = afterScale;
		target.transform.position = afterPosition;

		return target;
	}

	void ICommandBase.Execute(bool isRedo) => Execute(isRedo);

	public void Undo()
	{
		CheckTarget();

		ObjectSelection.Instance.DeselectObject();
		EditorHUDManager.Instance.CloseDispenserEditor();
		EditorHUDManager.Instance.CloseInteractionEditor();

		target.transform.localScale = beforeScale;
		target.transform.position = beforePosition;
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
