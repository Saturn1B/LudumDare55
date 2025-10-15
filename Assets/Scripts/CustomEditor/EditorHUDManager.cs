using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;

[System.Serializable]
public enum GizmoMode
{
	MOVEABLE = 0,
	SCALEABLE = 1,
	PIVOTABLE = 2
}

public class EditorHUDManager : MonoBehaviour
{
	public static EditorHUDManager Instance { get; private set; }

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

	private int selectedId;

	private void Start()
	{
		SwitchGizmoMode((int)GizmoMode.MOVEABLE);
		PopulateObjectSelecter();
	}

	[Header("Menu Mode")]
	[SerializeField] private GameObject menuPanel;
	public bool isPaused { get; private set; }

	public void PauseGame()
	{
		isPaused = !isPaused;
		menuPanel.SetActive(isPaused);
		if (isPaused)
		{
			ObjectSelection.Instance.DeselectObject();
			ObjectPlacer.Instance.SetCurrentObject(null);
		}
	}
	public void SaveLevel(bool unPause)
	{
		SaveSystem.Instance.SaveOnDisk();
		if(unPause)
			PauseGame();
	}
	public void MainMenu()
	{
		LevelDataTransfer.isEditing = false;
		LevelDataTransfer.SceneDataToTest = null;
		SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
	}
	public void TestLevel()
	{
		LevelDataTransfer.SceneDataToTest = SaveSystem.Instance.SaveSceneData();
		SceneManager.LoadScene("PlayScene", LoadSceneMode.Single);
	}

	[Space]

	[Header("Tool Mode")]
	[SerializeField] private GameObject grid;
	private bool isGridOn = true;
	[SerializeField] private Sprite gridOn, gridOff;
	[SerializeField] private Image gridIcon;
	[SerializeField] private Transform targetObject;
	[SerializeField] private FreeEditorCam camObject;

	public void ToggleGrid()
	{
		if (isGridOn)
		{
			isGridOn = false;
			gridIcon.sprite = gridOff;
		}
		else
		{
			isGridOn = true;
			gridIcon.sprite = gridOn;
		}

		grid.SetActive(isGridOn);

	}
	public void Recenter()
	{
		Transform target = targetObject;

		//Recenter on selected object if found one else recenter on spawn point
		if(ObjectSelection.Instance.selected != null)
		{
			target = ObjectSelection.Instance.selected;
		}

		Transform camHolderObject = camObject.transform.parent;
		Vector3 directionFromTarget = (camHolderObject.position - target.position).normalized;
		camHolderObject.position = target.position + directionFromTarget * 10;

		camObject.SetCamRotation(target);
	}

	[Space]

	private GizmoMode _gizmoMode;
	[Header("Mouse Drag Mode")]
	[SerializeField] private GameObject[] mouseDragModeImages;
	[SerializeField] private UnityEngine.UI.Button translateButton, scaleButton, rotationButton;
	private bool isGizmoButtonActive = true;

	public void SwitchGizmoMode(int mode = -1)
	{
		if(mode == -1)
		{
			foreach (var gizmoMode in mouseDragModeImages)
			{
				gizmoMode.SetActive(false);
			}
			return;
		}

		GizmoGestion.Instance.RefreshGizmoMode((GizmoMode)mode);

		for (int i = 0; i < mouseDragModeImages.Length; i++)
		{
			if (i == (int)mode)
				mouseDragModeImages[i].SetActive(true);
			else
				mouseDragModeImages[i].SetActive(false);
		}

		if (selectedId == 0) return;

		SwitchCurrentObject(0);
		ObjectPlacer.Instance.SetCurrentObject(null);
	}

	public void GizmoSelectionButton(bool translateState, bool scaleState, bool rotationState)
	{
		translateButton.interactable = translateState;
		scaleButton.interactable = scaleState;
		rotationButton.interactable = rotationState;
	}

	private void SwitchGizmoButtonActiveState()
	{
		SwitchGizmoMode();

		isGizmoButtonActive = !isGizmoButtonActive;

		translateButton.interactable = isGizmoButtonActive;
		scaleButton.interactable = isGizmoButtonActive;
		rotationButton.interactable = isGizmoButtonActive;
	}

	public void OpenCursorTooltip(string objectName, bool canScale = false)
	{
		tooltipsCursor.OpenTooltips(objectName, canScale);
	}
	public void CloseCursorTooltip()
	{
		tooltipsCursor.CloseTooltips();
	}

	[Space]

	[Header("Object Selecter")]
	[SerializeField] private Transform objectSelecterTransform;
	[SerializeField] private GameObject objectButtonPrefab;
	private UnityEngine.Object[] sceneObjectSOs;
	[SerializeField] private List<ObjectButton> objectButtons;
	[SerializeField] private List<GameObject> typeSelectionButtons;
	[SerializeField] private Color highColor, lowColor;
	[SerializeField] private TooltipsCursor tooltipsCursor;
	private bool isTypeButtonActive = true;
	private int toolModeCount;

	private void PopulateObjectSelecter()
	{
		toolModeCount = objectButtons.Count;

		sceneObjectSOs = Resources.LoadAll("SceneObjects", typeof(SceneObjectSO));

		foreach (var sObject in sceneObjectSOs)
		{
			SceneObjectSO currentObject = (SceneObjectSO)sObject;

			if (currentObject.objectType == SceneObjectType.HIDE)
				continue;

			GameObject o = Instantiate(objectButtonPrefab, objectSelecterTransform);
			o.GetComponent<ObjectButton>().ButtonSetter(currentObject.objectName, currentObject.objectPrefab, currentObject.objectSprite);
			objectButtons.Add(o.GetComponent<ObjectButton>());
		}

		for (int i = 0; i < objectButtons.Count; i++)
		{
			objectButtons[i].id = i;
		}

		SwitchCurrentObject(0);
	}
	public void PopulateObjectSelecter(int objectType)
	{

		for (int i = objectButtons.Count - 1; i >= toolModeCount; i--)
		{
			ObjectButton toDelete = objectButtons[i];
			objectButtons.RemoveAt(i);
			Destroy(toDelete.transform.gameObject);
		}

		sceneObjectSOs = Resources.LoadAll("SceneObjects", typeof(SceneObjectSO));

		foreach (var sObject in sceneObjectSOs)
		{
			SceneObjectSO currentObject = (SceneObjectSO)sObject;

			if (currentObject.objectType == SceneObjectType.HIDE)
				continue;

			if ((SceneObjectType)objectType != SceneObjectType.ALL && currentObject.objectType != (SceneObjectType)objectType)
				continue;

			GameObject o = Instantiate(objectButtonPrefab, objectSelecterTransform);
			o.GetComponent<ObjectButton>().ButtonSetter(currentObject.objectName, currentObject.objectPrefab, currentObject.objectSprite);
			objectButtons.Add(o.GetComponent<ObjectButton>());
		}

		for (int i = 0; i < objectButtons.Count; i++)
		{
			objectButtons[i].id = i;
		}

		SwitchCurrentObject(0);
	}

	public void SetColor(GameObject thisButton)
	{
		foreach (GameObject b in typeSelectionButtons)
		{
			b.GetComponent<Image>().color = lowColor;
			b.GetComponentInChildren<TMP_Text>().color = highColor;
		}

		thisButton.GetComponent<Image>().color = highColor;
		thisButton.GetComponentInChildren<TMP_Text>().color = lowColor;
	}

	public void SwitchCurrentObject(int id)
	{
		selectedId = id;
		ObjectButton sButton = null;
		foreach (var sObject in objectButtons)
		{
			if (sObject.id == id)
			{
				sObject.ChangeSelectedState(true);
				sButton = sObject;
			}
			else
				sObject.ChangeSelectedState(false);
		}

		if (id == 2)
			ObjectPlacer.Instance.isDelete = true;
		else
			ObjectPlacer.Instance.isDelete = false;

		if (id == 1)
			ObjectPlacer.Instance.isEditor = true;
		else
			ObjectPlacer.Instance.isEditor = false;

		if (id == 0 || id == 1 || id == 2)
			ObjectPlacer.Instance.SetCurrentObject(null);
		else
			ObjectPlacer.Instance.SetCurrentObject(sButton.objectPrefab);

	}

	private void SwitchTypeButtonActiveState()
	{
		isTypeButtonActive = !isTypeButtonActive;

		foreach (var button in typeSelectionButtons)
		{
			button.GetComponent<UnityEngine.UI.Button>().interactable = isTypeButtonActive;
		}
	}

	[Space]

	[Header("Interaction Editor")]
	[SerializeField] private GameObject editorPanel;
	[SerializeField] private TMP_Text editorTitle;
	[SerializeField] private TMP_Text activatorName;
	[SerializeField] private GameObject interactableLister;
	[SerializeField] private Transform activableListPanel;
	private List<InteractableLister> interactables = new List<InteractableLister>();
	[HideInInspector] public UnityEvent _openInteractionEditor;
	[HideInInspector] public UnityEvent _closeInteractionEditor;
	[SerializeField] private GameObject connectionLinePrefab;
	[SerializeField] private GameObject quickMenu;
	private List<GameObject> connectionLines = new List<GameObject>();

	[Header("Tootlips")]
	[SerializeField] private TooltipsOption tooltipsOption;
	[SerializeField] private string activatorSubtitle;
	[SerializeField, TextArea(10, 10)] private string activatorExplanation;
	[SerializeField] private string activableSubtitle;
	[SerializeField, TextArea(10, 10)] private string activableExplanation;

	[HideInInspector] public bool interactionEditorOpen;

	public void OpenInteractionEditor(ActivatorEditor activatorEditor)
	{
		interactionEditorOpen = true;

		editorPanel.SetActive(true);
		quickMenu.SetActive(false);
		editorTitle.text = activatorSubtitle;
		activatorName.text = "> " + activatorEditor.activatorName;

		tooltipsOption.subtitle = activatorSubtitle;
		tooltipsOption.explanation = activatorExplanation;

		foreach (var activable in activatorEditor.activables)
		{
			GameObject go = Instantiate(interactableLister, activableListPanel);
			go.GetComponent<InteractableLister>().Setup(activable);
			interactables.Add(go.GetComponent<InteractableLister>());

			CreateConnectionLine(activatorEditor.transform.position, activable.transform.position);

		}

		_openInteractionEditor.Invoke();
		SwitchGizmoButtonActiveState();
		SwitchTypeButtonActiveState();
	}
	public void OpenInteractionEditor(ActivableEditor activableEditor)
	{
		interactionEditorOpen = true;

		editorPanel.SetActive(true);
		quickMenu.SetActive(false);
		editorTitle.text = activableSubtitle;
		activatorName.text = "> " + activableEditor.activableName;

		tooltipsOption.subtitle = activableSubtitle;
		tooltipsOption.explanation = activableExplanation;

		foreach (var activator in activableEditor.activators)
		{
			GameObject go = Instantiate(interactableLister, activableListPanel);
			go.GetComponent<InteractableLister>().Setup(activator);
			interactables.Add(go.GetComponent<InteractableLister>());

			CreateConnectionLine(activableEditor.transform.position, activator.transform.position);
		}

		_openInteractionEditor.Invoke();
		SwitchGizmoButtonActiveState();
		SwitchTypeButtonActiveState();
	}

	public void CreateConnectionLine(Vector3 startPos, Vector3 endPos)
	{
		GameObject cl = Instantiate(connectionLinePrefab);
		connectionLines.Add(cl);
		LineRenderer clRenderer = cl.GetComponent<LineRenderer>();
		clRenderer.SetPosition(0, startPos);
		clRenderer.SetPosition(1, endPos);
	}

	public void RemoveConnectionLineAt(Vector3 endPos)
	{
		GameObject clToRemove = null;
		foreach (var cl in connectionLines)
		{
			if(cl.GetComponent<LineRenderer>().GetPosition(1) == endPos)
			{
				clToRemove = cl;
				break;
			}
		}

		if(clToRemove != null)
		{
			connectionLines.Remove(clToRemove);
			Destroy(clToRemove);
		}
	}

	public void CloseInteractionEditor()
	{
		interactionEditorOpen = false;

		HideAllCurrentActivableHighlight();

		for (int i = activableListPanel.transform.childCount - 1; i >= 0; i--)
		{
			Destroy(activableListPanel.transform.GetChild(i).gameObject);
		}

		interactables.Clear();

		foreach (var cl in connectionLines)
		{
			Destroy(cl);
		}

		connectionLines.Clear();

		editorPanel.SetActive(false);
		quickMenu.SetActive(true);
		activatorName.text = "";
		ObjectSelection.Instance.RemoveCurrentActivator();
		ObjectSelection.Instance.editorOpened = false;

		_closeInteractionEditor.Invoke();
		SwitchGizmoButtonActiveState();
		SwitchTypeButtonActiveState();
	}

	public void AddActivableInteractionEditor(ActivableEditor activableEditor)
	{
		foreach (var acti in ObjectSelection.Instance.GetCurrentActivator().activables)
		{
			if (acti == activableEditor)
			{
				return;
			}
		}

		List<ActivableEditor> startActivables = ListCloner.CloneMonoBehaviourListReference(ObjectSelection.Instance.GetCurrentActivator().activables); //HERE
		List<ActivatorEditor> startActivators = ListCloner.CloneMonoBehaviourListReference(activableEditor.activators); //HERE

		List<ActivableEditor> endActivables = ListCloner.CloneMonoBehaviourListReference(startActivables); //HERE
		List<ActivatorEditor> endActivators = ListCloner.CloneMonoBehaviourListReference(startActivators); //HERE
		endActivables.Add(activableEditor);
		endActivators.Add(ObjectSelection.Instance.GetCurrentActivator());

		ActivatorCommand activatorCommand = new ActivatorCommand(
			ObjectSelection.Instance.GetCurrentActivator().gameObject, ObjectSelection.Instance.GetCurrentActivator().transform.GetComponent<ModifiableObject>().objectId,
			activableEditor.gameObject, activableEditor.transform.GetComponent<ModifiableObject>().objectId,
			startActivables, startActivators, endActivables, endActivators);

		HystoryCommand.Instance.ExecuteCommand(activatorCommand);

		AddActivableUILister(activableEditor);
		CreateConnectionLine(ObjectSelection.Instance.GetCurrentActivator().transform.position, activableEditor.transform.position);

		//ObjectSelection.Instance.GetCurrentActivator().activables.Add(activableEditor);
		//activableEditor.activators.Add(ObjectSelection.Instance.GetCurrentActivator());
	}
	public void AddActivableUILister(ActivableEditor activableEditor)
	{
		GameObject activable = Instantiate(interactableLister, activableListPanel);
		activable.GetComponent<InteractableLister>().Setup(activableEditor);
		interactables.Add(activable.GetComponent<InteractableLister>());
	}

	public void RemoveActivableInteractionEditor(InteractableLister activableLister)
	{
		interactables.Remove(activableLister);
		RemoveConnectionLineAt(activableLister.activableEditor.transform.position);

		List<ActivableEditor> startActivables = ListCloner.CloneMonoBehaviourListReference(ObjectSelection.Instance.GetCurrentActivator().activables); //HERE
		List<ActivatorEditor> startActivators = ListCloner.CloneMonoBehaviourListReference(activableLister.activableEditor.activators); //HERE

		List<ActivableEditor> endActivables = ListCloner.CloneMonoBehaviourListReference(startActivables); //HERE
		List<ActivatorEditor> endActivators = ListCloner.CloneMonoBehaviourListReference(startActivators); //HERE
		endActivables.Remove(activableLister.activableEditor);
		endActivators.Remove(ObjectSelection.Instance.GetCurrentActivator());

		ActivatorCommand activatorCommand = new ActivatorCommand(
			ObjectSelection.Instance.GetCurrentActivator().gameObject, ObjectSelection.Instance.GetCurrentActivator().transform.GetComponent<ModifiableObject>().objectId,
			activableLister.activableEditor.gameObject, activableLister.activableEditor.transform.GetComponent<ModifiableObject>().objectId,
			startActivables, startActivators, endActivables, endActivators);

		HystoryCommand.Instance.ExecuteCommand(activatorCommand);

		//ObjectSelection.Instance.GetCurrentActivator().activables.Remove(interactableLister.activableEditor);
		//interactableLister.activableEditor.activators.Remove(ObjectSelection.Instance.GetCurrentActivator());
	}


	public void HideAllCurrentActivableHighlight()
	{
		foreach (var interactable in interactables)
		{
			interactable.RemoveHighlight();
		}
	}

	[Space]

	[Header("Dispenser Editor")]
	[SerializeField] private GameObject dispenserPanel;
	[SerializeField] private TMP_Dropdown typeDropdown, numberDropdown;
	private DispenserEditor currentDispenserEditor;

	[HideInInspector] public bool dispenserEditorOpen;

	public void OpenDispenserEditor(DispenserEditor dispenserEditor)
	{
		dispenserEditorOpen = true;

		dispenserPanel.SetActive(true);
		quickMenu.SetActive(false);
		currentDispenserEditor = dispenserEditor;
		typeDropdown.value = (int)dispenserEditor.materialType - 1;
		numberDropdown.value = dispenserEditor.materialNumber;

		_openInteractionEditor.Invoke();
		SwitchGizmoButtonActiveState();
		SwitchTypeButtonActiveState();
	}

	public void CloseDispenserEditor()
	{
		dispenserEditorOpen = false;

		dispenserPanel.SetActive(false);
		quickMenu.SetActive(true);

		ObjectSelection.Instance.editorOpened = false;
		currentDispenserEditor = null;

		_closeInteractionEditor.Invoke();
		SwitchGizmoButtonActiveState();
		SwitchTypeButtonActiveState();
	}

	public void ChangeDispenserType(int type)
	{
		currentDispenserEditor.SetMaterialCommand(type);
	}

	public void ChangeDispenserNumber(int number)
	{
		currentDispenserEditor.SetNumberCommand(number);
	}
}