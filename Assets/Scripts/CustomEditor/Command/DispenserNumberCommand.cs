using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispenserNumberCommand : ICommand
{
	private GameObject target;
	private string targetId;
	private int beforeMaterialNumber;
	private int afterMaterialNumber;

	public DispenserNumberCommand(GameObject target, string targetId, int beforeMaterialNumber, int afterMaterialNumber)
	{
		this.target = target;
		this.targetId = targetId;
		this.beforeMaterialNumber = beforeMaterialNumber;
		this.afterMaterialNumber = afterMaterialNumber;
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

		DispenserEditor targetDispenserEditor = target.GetComponent<DispenserEditor>();
		targetDispenserEditor.SetNumber(afterMaterialNumber);
	}

	public void Undo()
	{
		CheckTarget();

		ObjectSelection.Instance.DeselectObject();
		EditorHUDManager.Instance.CloseDispenserEditor();
		EditorHUDManager.Instance.CloseInteractionEditor();

		DispenserEditor targetDispenserEditor = target.GetComponent<DispenserEditor>();
		targetDispenserEditor.SetNumber(beforeMaterialNumber);
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
