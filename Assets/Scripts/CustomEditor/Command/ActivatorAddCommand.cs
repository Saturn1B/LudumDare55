using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActivatorCommand : ICommand
{
	private GameObject activatorTarget;
	private string activatorTargetId;

	private GameObject activableTarget;
	private string activableTargetId;

	private List<ActivableEditor> beforeActivables = new List<ActivableEditor>();
	private List<ActivatorEditor> beforeActivators = new List<ActivatorEditor>();

	private List<ActivableEditor> afterActivables = new List<ActivableEditor>();
	private List<ActivatorEditor> afterActivators = new List<ActivatorEditor>();

	private List<string> beforeActivablesId = new List<string>();
	private List<string> beforeActivatorsId = new List<string>();

	private List<string> afterActivablesId = new List<string>();
	private List<string> afterActivatorsId = new List<string>();

	public ActivatorCommand(GameObject activatorTarget, string activatorTargetId, GameObject activableTarget, string activableTargetId,
		List<ActivableEditor> beforeActivables, List<ActivatorEditor> beforeActivators,
		List<ActivableEditor> afterActivables, List<ActivatorEditor> afterActivators)
	{
		this.activatorTarget = activatorTarget;
		this.activatorTargetId = activatorTargetId;
		this.activableTarget = activableTarget;
		this.activableTargetId = activableTargetId;
		this.beforeActivables = beforeActivables;
		this.beforeActivators = beforeActivators;
		this.afterActivables = afterActivables;
		this.afterActivators = afterActivators;
		beforeActivablesId = SetIdList(beforeActivables);
		beforeActivatorsId = SetIdList(beforeActivators);
		afterActivablesId = SetIdList(afterActivables);
		afterActivatorsId = SetIdList(afterActivators);
	}

	public void Execute(bool isRedo)
	{
		CheckTarget();

		if (isRedo)
		{
			CheckLists();

			ObjectSelection.Instance.DeselectObject();
			EditorHUDManager.Instance.CloseDispenserEditor();
			EditorHUDManager.Instance.CloseInteractionEditor();
		}

		activatorTarget.GetComponent<ActivatorEditor>().activables = ListCloner.CloneMonoBehaviourListReference(afterActivables); //HERE
		activableTarget.GetComponent<ActivableEditor>().activators = ListCloner.CloneMonoBehaviourListReference(afterActivators); //HERE
	}

	public void Undo()
	{
		CheckTarget();
		CheckLists();

		ObjectSelection.Instance.DeselectObject();
		EditorHUDManager.Instance.CloseDispenserEditor();
		EditorHUDManager.Instance.CloseInteractionEditor();

		activatorTarget.GetComponent<ActivatorEditor>().activables = ListCloner.CloneMonoBehaviourListReference(beforeActivables); //HERE
		activableTarget.GetComponent<ActivableEditor>().activators = ListCloner.CloneMonoBehaviourListReference(beforeActivators); //HERE
	}

	private void CheckTarget()
	{
		if (activatorTarget == null && !string.IsNullOrEmpty(activatorTargetId))
		{
			foreach (var obj in SaveSystem.Instance.objectInScene)
			{
				if (obj.objectId == activatorTargetId)
				{
					activatorTarget = obj.gameObject;
					break;
				}
			}
		}

		if (activableTarget == null && !string.IsNullOrEmpty(activableTargetId))
		{
			foreach (var obj in SaveSystem.Instance.objectInScene)
			{
				if (obj.objectId == activableTargetId)
				{
					activableTarget = obj.gameObject;
					break;
				}
			}
		}
	}

	private void CheckLists()
	{
		for (int i = 0; i < beforeActivables.Count; i++)
		{
			if(beforeActivables[i] == null && !string.IsNullOrEmpty(beforeActivablesId[i]))
			{
				foreach (var obj in SaveSystem.Instance.objectInScene)
				{
					if (obj.objectId == beforeActivablesId[i])
					{
						beforeActivables[i] = obj.transform.GetComponent<ActivableEditor>();
						break;
					}
				}
			}
		}
		for (int i = 0; i < afterActivables.Count; i++)
		{
			if (afterActivables[i] == null && !string.IsNullOrEmpty(afterActivablesId[i]))
			{
				foreach (var obj in SaveSystem.Instance.objectInScene)
				{
					if (obj.objectId == afterActivablesId[i])
					{
						afterActivables[i] = obj.transform.GetComponent<ActivableEditor>();
						break;
					}
				}
			}
		}
		for (int i = 0; i < beforeActivators.Count; i++)
		{
			if (beforeActivators[i] == null && !string.IsNullOrEmpty(beforeActivatorsId[i]))
			{
				foreach (var obj in SaveSystem.Instance.objectInScene)
				{
					if (obj.objectId == beforeActivatorsId[i])
					{
						beforeActivators[i] = obj.transform.GetComponent<ActivatorEditor>();
						break;
					}
				}
			}
		}
		for (int i = 0; i < afterActivators.Count; i++)
		{
			if (afterActivators[i] == null && !string.IsNullOrEmpty(afterActivatorsId[i]))
			{
				foreach (var obj in SaveSystem.Instance.objectInScene)
				{
					if (obj.objectId == afterActivatorsId[i])
					{
						afterActivators[i] = obj.transform.GetComponent<ActivatorEditor>();
						break;
					}
				}
			}
		}
	}

	public List<string> SetIdList<T>(List<T> originalList) where T : MonoBehaviour
	{
		List<string> idList = new List<string>(originalList.Count);

		foreach (T item in originalList)
		{
			idList.Add(item.GetComponent<ModifiableObject>().objectId);
		}

		return idList;
	}
}
