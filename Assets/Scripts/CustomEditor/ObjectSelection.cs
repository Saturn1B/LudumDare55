using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectSelection : MonoBehaviour
{
    [HideInInspector] public Transform selected;
    [HideInInspector] public bool mouseOverDragUI;
    [HideInInspector] public bool mouseOverEditorUI;
    public LayerMask gizmoLayer;

    [HideInInspector] public bool editorOpened = false;
    private ActivatorEditor currentActivator;
    public ActivatorEditor GetCurrentActivator() { return currentActivator; }
    public void RemoveCurrentActivator() { currentActivator = null; }

    public static ObjectSelection Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    public void DeselectObject()
	{
        if (selected != null)
        {
            selected.gameObject.GetComponent<Outline>().enabled = false;

            if (selected.GetComponent<ModifiableObject>().hasChildObjects)
            {
                ModifiableObject selectedModifiable = selected.GetComponent<ModifiableObject>();

                foreach (ModifiableObject childObject in selectedModifiable.childObjects)
                {
                    childObject.gameObject.GetComponent<Outline>().enabled = false;
                }
            }
        }

        selected = null;
        GizmoGestion.Instance.DeactivateGizmo();
        EditorHUDManager.Instance.GizmoSelectionButton(true, true, true);
    }

    public void SelectObject(Transform toSelect)
	{
        if (selected == toSelect)
        {
            return;
        }

        if (selected != null)
        {
            selected.gameObject.GetComponent<Outline>().enabled = false;

            if (selected.GetComponent<ModifiableObject>().hasChildObjects)
            {
                ModifiableObject selectedModifiable = selected.GetComponent<ModifiableObject>();

                foreach (ModifiableObject childObject in selectedModifiable.childObjects)
                {
                    childObject.gameObject.GetComponent<Outline>().enabled = false;
                }
            }

            GizmoGestion.Instance.ActivateGizmo(selected.GetComponent<ModifiableObject>());
        }

        selected = toSelect;
        if (selected.gameObject.GetComponent<Outline>() != null)
        {
            selected.gameObject.GetComponent<Outline>().enabled = true;
        }
        else
        {
            Outline outline = selected.gameObject.AddComponent<Outline>();
            outline.enabled = true;
            selected.gameObject.GetComponent<Outline>().OutlineColor = Color.magenta;
            selected.gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
        }

		if (selected.GetComponent<ModifiableObject>().hasChildObjects)
		{
            ModifiableObject selectedModifiable = selected.GetComponent<ModifiableObject>();

			foreach (ModifiableObject childObject in selectedModifiable.childObjects)
			{
                if (childObject.gameObject.GetComponent<Outline>() != null)
                {
                    childObject.gameObject.GetComponent<Outline>().enabled = true;
                }
                else
                {
                    Outline outline = childObject.gameObject.AddComponent<Outline>();
                    outline.enabled = true;
                    childObject.gameObject.GetComponent<Outline>().OutlineColor = Color.magenta;
                    childObject.gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
                }
            }
        }

        GizmoGestion.Instance.ActivateGizmo(selected.GetComponent<ModifiableObject>());
    }

    void Update()
    {
        if (EditorHUDManager.Instance.isPaused) return;

        if (mouseOverDragUI) return;
        if (mouseOverEditorUI) return;
        if (ObjectPlacer.Instance.mouseOverSelecterUI) return;

        if ((ObjectPlacer.Instance.GetSelectionMode() == SelectionMode.NONE || ObjectPlacer.Instance.GetSelectionMode() == SelectionMode.EDITOR) && Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if(ObjectPlacer.Instance.GetSelectionMode() == SelectionMode.NONE)
			{
                RaycastHit gizmoHit;
                if (Physics.Raycast(ray, out gizmoHit, 100, gizmoLayer))
                {
                    if (gizmoHit.transform != null) return;
                }

                if (Physics.Raycast(ray, out hit, 100))
                {
                    if (hit.transform.CompareTag("Selectable"))
                    {
                        SelectObject(hit.transform);
                    }
                    else if (hit.transform.gameObject.layer == 8)
                    {
                        //DO NOTHING
                    }
                    else
                    {
                        DeselectObject();
                    }
                }
                else
                {
                    DeselectObject();
                }
            }
            else if(ObjectPlacer.Instance.GetSelectionMode() == SelectionMode.EDITOR)
			{
                if (Physics.Raycast(ray, out hit, 100))
				{
                    if (hit.transform.GetComponent<ActivatorEditor>())
					{
                        if (!currentActivator && !editorOpened)
						{
                            currentActivator = hit.transform.GetComponent<ActivatorEditor>();

                            EditorHUDManager.Instance.OpenInteractionEditor(currentActivator);
                            editorOpened = true;
                        }


                    }
                    else if (hit.transform.GetComponent<ActivableEditor>())
					{
                        ActivableEditor currentActivable = hit.transform.GetComponent<ActivableEditor>();

                        if (currentActivator)
						{
                            if (currentActivable.activators.Count == 0)
                                EditorHUDManager.Instance.AddActivableInteractionEditor(hit.transform.GetComponent<ActivableEditor>());
							else
							{
                                if (currentActivable.activators[0] = currentActivator)
                                    DisplayMessage.Instance.WarningMessage($"Activable - {currentActivable.activableName} - already linked to this Activator");

                                else
                                    DisplayMessage.Instance.ErrorMessage($"Activable - {currentActivable.activableName} - already linked to another Activator - {currentActivable.activators[0].activatorName} -");
                            }
						}
						else if (!editorOpened)
						{
                            EditorHUDManager.Instance.OpenInteractionEditor(currentActivable);
                            editorOpened = true;
                        }
                    }
                    else if (hit.transform.GetComponent<DispenserEditor>())
					{
						if (!editorOpened)
						{
                            EditorHUDManager.Instance.OpenDispenserEditor(hit.transform.GetComponent<DispenserEditor>());
                            editorOpened = true;
						}
					}
				}
            }
        }
    }
}
