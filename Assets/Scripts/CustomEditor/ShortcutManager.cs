using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShortcutManager : MonoBehaviour
{
	public static bool pauseDisponible = true;

    // Update is called once per frame
    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (ObjectSelection.Instance.selected != null)
				ObjectSelection.Instance.DeselectObject();
			else if (ObjectPlacer.Instance.GetSelectionMode() == SelectionMode.OBJECT)
			{
				ObjectPlacer.Instance.SetCurrentObject(null);
				EditorHUDManager.Instance.SwitchGizmoMode(0);
			}
			else if (pauseDisponible)
				EditorHUDManager.Instance.PauseGame();
		}

		if (EditorHUDManager.Instance.isPaused) return;

		//Gizmo mode shortcut
		//Translate shortcut
		if (Input.GetKeyDown(KeyCode.W) && ObjectPlacer.Instance.GetSelectionMode() == SelectionMode.NONE)
		{
			if (ObjectSelection.Instance.selected != null)
			{
				if (ObjectSelection.Instance.selected.GetComponent<ModifiableObject>().canTranslate)
					EditorHUDManager.Instance.SwitchGizmoMode(0);
			}
			else
			{
				EditorHUDManager.Instance.SwitchGizmoMode(0);
			}
		}
		//Scale shortcut
		if (Input.GetKeyDown(KeyCode.R) && ObjectPlacer.Instance.GetSelectionMode() == SelectionMode.NONE)
		{
			if (ObjectSelection.Instance.selected != null)
			{
				if (ObjectSelection.Instance.selected.GetComponent<ModifiableObject>().canScale)
					EditorHUDManager.Instance.SwitchGizmoMode(1);
			}
			else
			{
				EditorHUDManager.Instance.SwitchGizmoMode(1);
			}
		}
		//Rotate shortcut
		if (Input.GetKeyDown(KeyCode.E) && ObjectPlacer.Instance.GetSelectionMode() == SelectionMode.NONE)
		{
			if(ObjectSelection.Instance.selected != null)
			{
				if(ObjectSelection.Instance.selected.GetComponent<ModifiableObject>().canRotate)
					EditorHUDManager.Instance.SwitchGizmoMode(2);
			}
			else
			{
				EditorHUDManager.Instance.SwitchGizmoMode(2);
			}
		}

		if (Input.GetKeyDown(KeyCode.Delete))
		{
			if (ObjectPlacer.Instance.GetSelectionMode() == SelectionMode.EDITOR || ObjectSelection.Instance.selected != null)
			{
				GameObject toDelete = ObjectSelection.Instance.selected.gameObject;

				ObjectSelection.Instance.DeselectObject();

				ObjectPlacer.Instance.DeleteObject(toDelete);
			}
		}

		if (Input.GetKey(KeyCode.LeftControl))
		{
			//Save shortcut
			if (Input.GetKeyDown(KeyCode.S))
			{
				SaveSystem.Instance.SaveOnDisk();
				DisplayMessage.Instance.NormalMessage($"Saved Level - {LevelDataTransfer.levelName}");
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

					ModifiableObject duplicate = ObjectPlacer.Instance.CreateObject(toDuplicate.position + Vector3.forward + Vector3.right, toDuplicate.transform.rotation, toDuplicate.transform.localScale, toDuplicate.gameObject, toDuplicate.name);

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
				DisplayMessage.Instance.NormalMessage("Undo last action");
			}
			//Redo shortcut
			else if (Input.GetKeyDown(KeyCode.Y))
			{
				HystoryCommand.Instance.Redo();
				DisplayMessage.Instance.NormalMessage($"Redo last action");
			}
		}
	}
}
