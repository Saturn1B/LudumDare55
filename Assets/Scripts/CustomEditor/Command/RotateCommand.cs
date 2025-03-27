using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCommand : ICommand
{
	private GameObject target;
	private string targetId;
	private Quaternion beforeRotation;
	private Quaternion afterRotation;

	public RotateCommand(GameObject target, string targetId, Quaternion beforeRotation, Quaternion afterRotation)
	{
		this.target = target;
		this.targetId = targetId;
		this.beforeRotation = beforeRotation;
		this.afterRotation = afterRotation;
	}

	public void Execute(bool isRedo)
	{
		CheckTarget();

		if (isRedo)
		{
			ObjectSelection.Instance.DeselectObject();
			EditorHUDManager.Instance.CloseDispenserEditor();
			EditorHUDManager.Instance.CloseInteractionEditor();
		}

		target.transform.rotation = afterRotation;
	}

	public void Undo()
	{
		CheckTarget();

		ObjectSelection.Instance.DeselectObject();
		EditorHUDManager.Instance.CloseDispenserEditor();
		EditorHUDManager.Instance.CloseInteractionEditor();

		target.transform.rotation = beforeRotation;
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
