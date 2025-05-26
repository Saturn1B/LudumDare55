using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortcutManager : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			EditorHUDManager.Instance.PauseGame();
		}

		if (EditorHUDManager.Instance.isPaused) return;

		if (Input.GetKey(KeyCode.LeftControl))
		{
			//Save shortcut
			if (Input.GetKeyDown(KeyCode.S))
			{
				SaveSystem.Instance.SaveOnDisk();
			}
			//Duplicate shortcut
			else if (Input.GetKeyDown(KeyCode.D))
			{
				if(ObjectPlacer.Instance.GetSelectionMode() == SelectionMode.EDITOR || ObjectSelection.Instance.selected != null)
				{
					if (ObjectSelection.Instance.selected.GetComponent<Undeletable>())
					{
						DisplayMessage.Instance.ErrorMessage($"- {ObjectSelection.Instance.selected.name} - cannot be duplicated");
						return;
					}

					Transform toDuplicate = ObjectSelection.Instance.selected;

					ObjectSelection.Instance.DeselectObject();

					ModifiableObject duplicate = ObjectPlacer.Instance.CreateObject(toDuplicate.position + Vector3.one, toDuplicate.rotation.eulerAngles, toDuplicate.localScale, toDuplicate.gameObject, toDuplicate.name);

					ObjectSelection.Instance.SelectObject(duplicate.transform);

					if (duplicate.GetComponent<ActivatorEditor>() != null)
					{
						duplicate.GetComponent<ActivatorEditor>().RemoveFromActivable();
						duplicate.GetComponent<ActivatorEditor>().activables.Clear();
					}

					if (duplicate.GetComponent<ActivableEditor>() != null)
					{
						duplicate.GetComponent<ActivableEditor>().RemoveFromActivator();
						duplicate.GetComponent<ActivableEditor>().activators.Clear();
					}
				}
			}
			//Undo shortcut
			else if (Input.GetKeyDown(KeyCode.Z))
			{
				HystoryCommand.Instance.Undo();
			}
			//Redo shortcut
			else if (Input.GetKeyDown(KeyCode.Y))
			{
				HystoryCommand.Instance.Redo();
			}
		}
	}
}
