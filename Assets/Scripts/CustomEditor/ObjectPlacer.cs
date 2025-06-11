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
	[SerializeField] private GameObject ghostObject;

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

		Ray testRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit testHit;
		if (Physics.Raycast(testRay, out testHit, 100))
		{
			if(!ghostObject.activeSelf)
				ghostObject.SetActive(true);
			if (currentObjectPrefab.GetComponent<MeshFilter>())
				ghostObject.GetComponent<MeshFilter>().mesh = currentObjectPrefab.GetComponent<MeshFilter>().sharedMesh;
			else
				ghostObject.GetComponent<MeshFilter>().mesh = null;
			for (int i = 0; i < currentObjectPrefab.transform.childCount; i++)
			{
				if (currentObjectPrefab.transform.GetChild(i).GetComponent<MeshFilter>())
				{
					ghostObject.transform.GetChild(i).gameObject.SetActive(true);
					ghostObject.transform.GetChild(i).GetComponent<MeshFilter>().mesh = currentObjectPrefab.transform.GetChild(i).GetComponent<MeshFilter>()?.sharedMesh;
					ghostObject.transform.GetChild(i).localPosition = currentObjectPrefab.transform.GetChild(i).localPosition;
					ghostObject.transform.GetChild(i).localRotation = currentObjectPrefab.transform.GetChild(i).localRotation;
					ghostObject.transform.GetChild(i).localScale = currentObjectPrefab.transform.GetChild(i).localScale;
				}
			}
			ghostObject.transform.position = SummonPointPrecal(testHit);
		}
		else
		{
			if (ghostObject.activeSelf)
			{
				ghostObject.SetActive(false);
				foreach (Transform child in ghostObject.transform)
				{
					child.gameObject.SetActive(false);
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit, 100))
			{
				if (isDelete)
				{
					DeleteObject(hit.transform.gameObject);
				}
				else
				{
					CreateObject(hit, currentObjectPrefab, currentObjectName);
				}
			}
		}
	}

	private Vector3 SummonPointPrecal(RaycastHit hit)
	{
		Vector3 objectSize = currentObjectPrefab.GetComponentInChildren<Renderer>().bounds.size;

		Vector3 offset = objectSize * 0.1f;

		Vector3 summonPoint = hit.point + hit.normal * offset.magnitude;
		summonPoint = new Vector3(Mathf.RoundToInt(summonPoint.x), Mathf.RoundToInt(summonPoint.y), Mathf.RoundToInt(summonPoint.z));

		if (currentObjectPrefab.GetComponent<ModifiableObject>() && !currentObjectPrefab.GetComponent<ModifiableObject>().isGroundOrWall)
			summonPoint -= Vector3.up * .5f;

		if (currentObjectPrefab.GetComponent<ModifiableObject>() && currentObjectPrefab.GetComponent<ModifiableObject>().isStuckToWall)
			summonPoint += Vector3.forward * .5f;

		return summonPoint;
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

	public void DeleteObject(GameObject toDelete)
	{
		if (!toDelete.transform.GetComponent<Undeletable>() && toDelete.transform.GetComponent<ModifiableObject>())
		{
			DeleteCommand deleteCommand;

			if (toDelete.transform.GetComponent<ActivableEditor>())
			{
				ActivableEditor currentActivable = toDelete.transform.GetComponent<ActivableEditor>();
				ActivableSave activableSave = new ActivableSave(currentActivable.activators);
				deleteCommand = new DeleteCommand(toDelete.transform.gameObject, toDelete.transform.GetComponent<ModifiableObject>().objectId,
					toDelete.transform.GetComponent<ModifiableObject>().parentPrefab, activableSave);
			}
			else if (toDelete.transform.GetComponent<ActivatorEditor>())
			{
				ActivatorEditor currentActivator = toDelete.transform.GetComponent<ActivatorEditor>();
				ActivatorSave activatorSave = new ActivatorSave(currentActivator.activables);
				deleteCommand = new DeleteCommand(toDelete.transform.gameObject, toDelete.transform.GetComponent<ModifiableObject>().objectId,
					toDelete.transform.GetComponent<ModifiableObject>().parentPrefab, activatorSave);
			}
			else if (toDelete.transform.GetComponent<DispenserEditor>())
			{
				DispenserEditor currentDispenser = toDelete.transform.GetComponent<DispenserEditor>();
				DispenserSave dispenserSave = new DispenserSave(currentDispenser.materialType, currentDispenser.materialNumber);
				deleteCommand = new DeleteCommand(toDelete.transform.gameObject, toDelete.transform.GetComponent<ModifiableObject>().objectId,
					toDelete.transform.GetComponent<ModifiableObject>().parentPrefab, dispenserSave);
			}
			else
			{
				deleteCommand = new DeleteCommand(toDelete.transform.gameObject, toDelete.transform.GetComponent<ModifiableObject>().objectId,
					toDelete.transform.GetComponent<ModifiableObject>().parentPrefab);
			}

			HystoryCommand.Instance.ExecuteCommand(deleteCommand);
		}
		else
		{
			DisplayMessage.Instance.ErrorMessage($"- {toDelete.transform.name} - cannot be removed");
		}
	}
}
