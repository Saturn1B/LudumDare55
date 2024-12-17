using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

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

	private GizmoMode _gizmoMode;
	[Header("Mouse Drag Mode")]
	[SerializeField] private GameObject[] mouseDragModeImages;
	[SerializeField] private UnityEngine.UI.Button translateButton, scaleButton, rotationButton;

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

	[Space]

	[Header("Object Selecter")]
	[SerializeField] private Transform objectSelecterTransform;
	[SerializeField] private GameObject objectButtonPrefab;
	private UnityEngine.Object[] sceneObjectSOs;
	[SerializeField] private List<ObjectButton> objectButtons;

	private void PopulateObjectSelecter()
	{
		sceneObjectSOs = Resources.LoadAll("SceneObjects", typeof(SceneObjectSO));

		foreach (var sObject in sceneObjectSOs)
		{
			SceneObjectSO currentObject = (SceneObjectSO)sObject;
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

	[Space]

	[Header("Interaction Editor")]
	[SerializeField] private GameObject editorPanel;
	[SerializeField] private TMP_Text activatorName;
	[SerializeField] private GameObject interactableLister;
	[SerializeField] private Transform activableListPanel;
	private List<InteractableLister> interactables = new List<InteractableLister>();
	[HideInInspector] public UnityEvent _openInteractionEditor;
	[HideInInspector] public UnityEvent _closeInteractionEditor;

	public void OpenInteractionEditor(ActivatorEditor activatorEditor)
	{
		editorPanel.SetActive(true);
		activatorName.text = "> " + activatorEditor.activatorName;

		foreach (var activable in activatorEditor.activables)
		{
			GameObject go = Instantiate(interactableLister, activableListPanel);
			go.GetComponent<InteractableLister>().Setup(activable);
			interactables.Add(go.GetComponent<InteractableLister>());
		}

		_openInteractionEditor.Invoke();
	}
	public void OpenInteractionEditor(ActivableEditor activableEditor)
	{
		editorPanel.SetActive(true);
		activatorName.text = "> " + activableEditor.activableName;

		foreach (var activator in activableEditor.activators)
		{
			GameObject go = Instantiate(interactableLister, activableListPanel);
			go.GetComponent<InteractableLister>().Setup(activator);
			interactables.Add(go.GetComponent<InteractableLister>());
		}

		_openInteractionEditor.Invoke();
	}

	public void CloseInteractionEditor()
	{
		HideAllCurrentActivableHighlight();

		for (int i = activableListPanel.transform.childCount - 1; i >= 0; i--)
		{
			Destroy(activableListPanel.transform.GetChild(i).gameObject);
		}

		interactables.Clear();

		editorPanel.SetActive(false);
		activatorName.text = "";
		ObjectSelection.Instance.RemoveCurrentActivator();
		ObjectSelection.Instance.editorOpened = false;

		_closeInteractionEditor.Invoke();
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

		GameObject activable = Instantiate(interactableLister, activableListPanel);
		activable.GetComponent<InteractableLister>().Setup(activableEditor);
		ObjectSelection.Instance.GetCurrentActivator().activables.Add(activableEditor);
		activableEditor.activators.Add(ObjectSelection.Instance.GetCurrentActivator());
		interactables.Add(activable.GetComponent<InteractableLister>());
	}

	public void RemoveActivableInteractionEditor(InteractableLister interactableLister)
	{
		ObjectSelection.Instance.GetCurrentActivator().activables.Remove(interactableLister.activableEditor);
		interactableLister.activableEditor.activators.Remove(ObjectSelection.Instance.GetCurrentActivator());
		interactables.Remove(interactableLister);
		Destroy(interactableLister.gameObject);
	}

	public void HideAllCurrentActivableHighlight()
	{
		foreach (var interactable in interactables)
		{
			interactable.RemoveHighlight();
		}
	}
}
