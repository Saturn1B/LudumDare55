using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectSelection : MonoBehaviour
{
    [HideInInspector] public Transform selected;
    [HideInInspector] public bool mouseOverDragUI;
    public LayerMask gizmoLayer;

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
        }

        selected = null;
        GizmoGestion.Instance.DeactivateGizmo();
        EditorHUDManager.Instance.GizmoSelectionButton(true, true, true);
    }

    void Update()
    {
        if (ObjectPlacer.Instance.GetSelectionMode() != SelectionMode.NONE) return;
        if (mouseOverDragUI) return;
        if (ObjectPlacer.Instance.mouseOverSelecterUI) return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            RaycastHit gizmoHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);


            if (Physics.Raycast(ray, out gizmoHit, 100, gizmoLayer))
			{
                if (gizmoHit.transform != null) return;
			}

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
			{
                if (hit.transform.CompareTag("Selectable"))
                {
                    if (selected == hit.transform)
					{
                        return;
                    }

                    if(selected != null)
					{
                        selected.gameObject.GetComponent<Outline>().enabled = false;
                        GizmoGestion.Instance.ActivateGizmo(selected.GetComponent<ModifiableObject>());
                    }

                    selected = hit.transform;
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

                    GizmoGestion.Instance.ActivateGizmo(selected.GetComponent<ModifiableObject>());
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
    }
}
