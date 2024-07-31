using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SelectionMode
{
	NONE = 0,
	DELETE = 1,
	OBJECT = 2
}

public class ObjectPlacer : MonoBehaviour
{
	public static ObjectPlacer Instance { get; private set; }

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

		freeEditorCam = GetComponent<FreeEditorCam>();
		eventSystem = FindObjectOfType<EventSystem>();
	}

	private FreeEditorCam freeEditorCam;
	private GameObject currentObjectPrefab;
	private EventSystem eventSystem;

	/*[HideInInspector]*/ public bool isDelete;
	[HideInInspector] public bool mouseOverSelecterUI;
	[HideInInspector] public bool mouseOverDragUI;

	public void SetCurrentObject(GameObject currentObject)
	{
		currentObjectPrefab = currentObject;
	}
	public SelectionMode GetSelectionMode()
	{
		SelectionMode _mode;

		if(currentObjectPrefab != null)
			_mode = SelectionMode.OBJECT;
		else if (isDelete)
			_mode = SelectionMode.DELETE;
		else
			_mode = SelectionMode.NONE;

		return _mode;
	}

	private void Update()
	{
		if (currentObjectPrefab == null && isDelete == false) return;
		if (mouseOverSelecterUI == true) return;
		if (mouseOverDragUI == true) return;

		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100))
			{
				if (isDelete)
				{
					if (!hit.transform.GetComponent<Undeletable>())
					{
						if (hit.transform.GetComponent<Scaleable>())
							ObjectRegister.Instance.scaleableObjects.Remove(hit.transform.GetComponent<Scaleable>());
						if (hit.transform.GetComponent<Moveable>())
							ObjectRegister.Instance.moveableObjects.Remove(hit.transform.GetComponent<Moveable>());
						Destroy(hit.transform.gameObject);
					}
				}
				else
				{
					Vector3 objectSize = currentObjectPrefab.GetComponentInChildren<Renderer>().bounds.size;

					Vector3 offset = objectSize * 0.1f;

					Vector3 summonPoint = hit.point + hit.normal * offset.magnitude;
					summonPoint = new Vector3(Mathf.RoundToInt(summonPoint.x), Mathf.RoundToInt(summonPoint.y), Mathf.RoundToInt(summonPoint.z));
					GameObject go = Instantiate(currentObjectPrefab, summonPoint, Quaternion.identity);
				}
			}
		}
	}
}
