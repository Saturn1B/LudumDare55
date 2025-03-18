using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SelectionMode
{
	NONE = 0,
	DELETE = 1,
	OBJECT = 2,
	EDITOR = 3
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
	private string currentObjectName;
	private EventSystem eventSystem;

	/*[HideInInspector]*/ public bool isDelete, isEditor;
	[HideInInspector] public bool mouseOverSelecterUI;
	[HideInInspector] public bool mouseOverDragUI;

	public void SetCurrentObject(GameObject currentObject, string objectName = "")
	{
		currentObjectPrefab = currentObject;
		currentObjectName = objectName;
	}
	public SelectionMode GetSelectionMode()
	{
		SelectionMode _mode;

		if(currentObjectPrefab != null)
			_mode = SelectionMode.OBJECT;
		else if (isDelete)
			_mode = SelectionMode.DELETE;
		else if (isEditor)
			_mode = SelectionMode.EDITOR;
		else
			_mode = SelectionMode.NONE;

		return _mode;
	}

	private void Update()
	{
		if (EditorHUDManager.Instance.isPaused) return;

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
					if (!hit.transform.GetComponent<Undeletable>() && hit.transform.GetComponent<ModifiableObject>())
					{
						if (hit.transform.GetComponent<ActivableEditor>())
						{
							ActivableEditor currentActivable = hit.transform.GetComponent<ActivableEditor>();
							currentActivable.RemoveFromActivator();
						}
						if (hit.transform.GetComponent<ActivatorEditor>())
						{
							ActivatorEditor currentActivator = hit.transform.GetComponent<ActivatorEditor>();
							currentActivator.RemoveFromActivable();
						}
						SaveSystem.Instance.objectInScene.Remove(hit.transform.GetComponent<ModifiableObject>());
						Destroy(hit.transform.gameObject);
					}
					else
					{
						DisplayMessage.Instance.ErrorMessage($"- {hit.transform.name} - cannot be removed");
					}
				}
				else
				{
					CreateObject(hit, currentObjectPrefab, currentObjectName);
				}
			}
		}
	}

	public void CreateObject(RaycastHit hit, GameObject objectPrefab, string objectName)
	{
		Vector3 objectSize = objectPrefab.GetComponentInChildren<Renderer>().bounds.size;

		Vector3 offset = objectSize * 0.1f;

		Vector3 summonPoint = hit.point + hit.normal * offset.magnitude;
		summonPoint = new Vector3(Mathf.RoundToInt(summonPoint.x), Mathf.RoundToInt(summonPoint.y), Mathf.RoundToInt(summonPoint.z));

		if (objectPrefab.GetComponent<ModifiableObject>() && !objectPrefab.GetComponent<ModifiableObject>().isGroundOrWall)
			summonPoint -= Vector3.up * .5f;

		if (objectPrefab.GetComponent<ModifiableObject>() && objectPrefab.GetComponent<ModifiableObject>().isStuckToWall)
			summonPoint += Vector3.forward * .5f;

		GameObject go = Instantiate(objectPrefab, summonPoint, Quaternion.identity);

		go.name = currentObjectName;

		SaveSystem.Instance.objectInScene.Add(go.GetComponent<ModifiableObject>());

		if (go.GetComponent<ActivatorEditor>())
		{
			go.GetComponent<ActivatorEditor>().activatorName = objectName;
		}
		if (go.GetComponent<ActivableEditor>())
		{
			go.GetComponent<ActivableEditor>().activableName = objectName;
		}
	}
	public ModifiableObject CreateObject(Vector3 position, Vector3 rotation, Vector3 scale, GameObject objectPrefab, string objectName)
	{
		GameObject go = Instantiate(objectPrefab);
		go.transform.position = position;
		go.transform.eulerAngles = rotation;
		go.transform.localScale = scale;

		SaveSystem.Instance.objectInScene.Add(go.GetComponent<ModifiableObject>());

		go.name = objectName;

		if (go.GetComponent<ActivatorEditor>())
		{
			go.GetComponent<ActivatorEditor>().activatorName = objectName;
		}
		if (go.GetComponent<ActivableEditor>())
		{
			go.GetComponent<ActivableEditor>().activableName = objectName;
		}

		return go.GetComponent<ModifiableObject>();
	}
}
