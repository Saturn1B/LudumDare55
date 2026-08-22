using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectButton : MonoBehaviour
{
	[HideInInspector] public GameObject objectPrefab;
	[SerializeField] private Image objectImage;
	public string objectName;
	[SerializeField] private GameObject outline;
	[HideInInspector] public int id;

	[Header("Specials")]
	[SerializeField] bool isNone;
	[SerializeField] bool isEditor;
	[SerializeField] bool isDelete;

	bool isButtonActive = true;

	private void Awake()
	{
		GetComponent<UnityEngine.UI.Button>().onClick.AddListener(SwitchToObject);
		EditorHUDManager.Instance._openInteractionEditor.AddListener(SwitchButtonActiveState);
		EditorHUDManager.Instance._closeInteractionEditor.AddListener(SwitchButtonActiveState);
	}

	public bool IsObjectOrMode()
	{
		if (isNone || isEditor || isDelete)
			return false;

		return true;
	}

	public void ButtonSetter(string objectName, GameObject objectPrefab, Sprite objectSprite)
	{
		this.objectPrefab = objectPrefab;
		objectImage.sprite = objectSprite;
		this.objectName = objectName;
		transform.name = $"Button_{this.objectName}";
	}

	private void SwitchToObject()
	{
		EditorHUDManager.Instance.SwitchCurrentObject(id);
		if (isNone)
		{
			ObjectPlacer.Instance.SetCurrentObject(null);
			EditorHUDManager.Instance.SwitchGizmoMode(0);
		}
		else if (isDelete || isEditor)
		{
			ObjectSelection.Instance.DeselectObject();
			ObjectPlacer.Instance.SetCurrentObject(null);
			EditorHUDManager.Instance.SwitchGizmoMode();
		}
		else
		{
			ObjectSelection.Instance.DeselectObject();
			ObjectPlacer.Instance.SetCurrentObject(objectPrefab, objectName);
			EditorHUDManager.Instance.SwitchGizmoMode();
		}

		ObjectPlacer.Instance.isDelete = isDelete;
		ObjectPlacer.Instance.isEditor = isEditor;
	}

	public void ChangeSelectedState(bool isSelected)
	{
		outline.SetActive(isSelected);
	}

	void SwitchButtonActiveState()
	{
		isButtonActive = !isButtonActive;
		GetComponent<UnityEngine.UI.Button>().interactable = isButtonActive;
	}
}