using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectSelection : MonoBehaviour
{
    [HideInInspector] public Transform selected;
    [HideInInspector] public bool mouseOverDragUI;

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

    void Update()
    {
        if (ObjectPlacer.Instance.GetSelectionMode() != SelectionMode.NONE) return;
        if (mouseOverDragUI) return;

        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100))
			{
                if (hit.transform.CompareTag("Selectable"))
                {
                    if (selected == hit.transform)
					{
                        SelectObject(selected, true);
                        return;
                    }

                    if(selected != null)
					{
                        selected.gameObject.GetComponent<Outline>().enabled = false;
                        SelectObject(selected, false);
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

                    SelectObject(selected, true);
                }
				else
				{
                    if (selected != null)
                    {
                        selected.gameObject.GetComponent<Outline>().enabled = false;
                        SelectObject(selected, false);
                    }

                    selected = null;
                }
            }
			else
			{
                if (selected != null)
                {
                    selected.gameObject.GetComponent<Outline>().enabled = false;
                    SelectObject(selected, false);
                }

                selected = null;
            }
        }
    }

    void SelectObject(Transform s, bool isSelected)
	{
        ObjectMouseDrag[] objectMouseDrags = s.GetComponents<ObjectMouseDrag>();
        foreach (var drag in objectMouseDrags)
        {
            drag.wasSelected = drag.isSelected;
            drag.isSelected = isSelected;
        }
    }
}
