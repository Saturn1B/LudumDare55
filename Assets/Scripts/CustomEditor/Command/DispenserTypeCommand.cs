using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispenserTypeCommand : ICommand
{
	private GameObject target;
	private string targetId;
	private Materials beforeMaterialType;
	private Materials afterMaterialType;

	public DispenserTypeCommand(GameObject target, string targetId, Materials beforeMaterialType, Materials afterMaterialType)
	{
		this.target = target;
		this.targetId = targetId;
		this.beforeMaterialType = beforeMaterialType;
		this.afterMaterialType = afterMaterialType;
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
		targetDispenserEditor.SetMaterial((int)afterMaterialType, false);
	}

	public void Undo()
	{
		CheckTarget();

		ObjectSelection.Instance.DeselectObject();
		EditorHUDManager.Instance.CloseDispenserEditor();
		EditorHUDManager.Instance.CloseInteractionEditor();

		DispenserEditor targetDispenserEditor = target.GetComponent<DispenserEditor>();
		targetDispenserEditor.SetMaterial((int)beforeMaterialType, false);
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
