using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public enum MouseDragMode
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
		SwitchMouseDragMode((int)MouseDragMode.MOVEABLE);
		PopulateObjectSelecter();
	}

	private MouseDragMode _mouseDragMode;
	[Header("Mouse Drag Mode")]
	[SerializeField] private GameObject[] mouseDragModeImages;

	public void SwitchMouseDragMode(int mode)
	{
		switch ((MouseDragMode)mode)
		{
			case MouseDragMode.MOVEABLE:
				ObjectRegister.Instance.SetMoveableMode();
				break;
			case MouseDragMode.SCALEABLE:
				ObjectRegister.Instance.SetScaleableMode();
				break;
			case MouseDragMode.PIVOTABLE:
				break;
			default:
				break;
		}

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

		if (id == 1)
			ObjectPlacer.Instance.isDelete = true;
		else
			ObjectPlacer.Instance.isDelete = false;

		if (id == 0 || id == 1)
			ObjectPlacer.Instance.SetCurrentObject(null);
		else
			ObjectPlacer.Instance.SetCurrentObject(sButton.objectPrefab);

	}
}
