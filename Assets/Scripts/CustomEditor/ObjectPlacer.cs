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
						//if (hit.transform.GetComponent<ActivableEditor>())
						//{
						//	ActivableEditor currentActivable = hit.transform.GetComponent<ActivableEditor>();
						//	currentActivable.RemoveFromActivator();
						//}
						//if (hit.transform.GetComponent<ActivatorEditor>())
						//{
						//	ActivatorEditor currentActivator = hit.transform.GetComponent<ActivatorEditor>();
						//	currentActivator.RemoveFromActivable();
						//}
						//SaveSystem.Instance.objectInScene.Remove(hit.transform.GetComponent<ModifiableObject>());
						//Destroy(hit.transform.gameObject);

						DeleteCommand deleteCommand;

						if (hit.transform.GetComponent<ActivableEditor>())
						{
							ActivableEditor currentActivable = hit.transform.GetComponent<ActivableEditor>();
							ActivableSave activableSave = new ActivableSave(currentActivable.activators);
							deleteCommand = new DeleteCommand(hit.transform.gameObject, hit.transform.GetComponent<ModifiableObject>().objectId,
								hit.transform.GetComponent<ModifiableObject>().parentPrefab, activableSave);
						}
						else if (hit.transform.GetComponent<ActivatorEditor>())
						{
							ActivatorEditor currentActivator = hit.transform.GetComponent<ActivatorEditor>();
							ActivatorSave activatorSave = new ActivatorSave(currentActivator.activables);
							deleteCommand = new DeleteCommand(hit.transform.gameObject, hit.transform.GetComponent<ModifiableObject>().objectId,
								hit.transform.GetComponent<ModifiableObject>().parentPrefab, activatorSave);
						}
						else if (hit.transform.GetComponent<DispenserEditor>())
						{
							DispenserEditor currentDispenser = hit.transform.GetComponent<DispenserEditor>();
							DispenserSave dispenserSave = new DispenserSave(currentDispenser.materialType, currentDispenser.materialNumber);
							deleteCommand = new DeleteCommand(hit.transform.gameObject, hit.transform.GetComponent<ModifiableObject>().objectId,
								hit.transform.GetComponent<ModifiableObject>().parentPrefab, dispenserSave);
						}
						else
						{
							deleteCommand = new DeleteCommand(hit.transform.gameObject, hit.transform.GetComponent<ModifiableObject>().objectId,
								hit.transform.GetComponent<ModifiableObject>().parentPrefab);
						}

						HystoryCommand.Instance.ExecuteCommand(deleteCommand);
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

	//Create object manually using command stack
	public void CreateObject(RaycastHit hit, GameObject objectPrefab, string objectName)
	{
		Debug.Log(objectName + " " + currentObjectName);

		CreateCommand createCommand = new CreateCommand(hit, objectPrefab, objectName, currentObjectName);

		HystoryCommand.Instance.ExecuteCommand(createCommand);
	}

	//Create object at scene loading
	public ModifiableObject CreateObject(Vector3 position, Vector3 rotation, Vector3 scale, GameObject objectPrefab, string objectName)
	{
		GameObject go = Instantiate(objectPrefab);
		go.transform.position = position;
		go.transform.eulerAngles = rotation;
		go.transform.localScale = scale;

		SaveSystem.Instance.objectInScene.Add(go.GetComponent<ModifiableObject>());

		go.name = objectName;

		go.GetComponent<ModifiableObject>().parentPrefab = objectPrefab;

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
