using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectButton : MonoBehaviour
{
	private GameObject objectPrefab;
	[SerializeField] private Image objectImage;
	[SerializeField] private GameObject outline;
	[HideInInspector] public int id;

	[Header("Specials")]
	[SerializeField] bool isNone;
	[SerializeField] bool isDelete;

	private void Awake()
	{
		GetComponent<UnityEngine.UI.Button>().onClick.AddListener(SwitchToObject);
	}

	public void ButtonSetter(string objectName, GameObject objectPrefab, Sprite objectSprite)
	{
		this.objectPrefab = objectPrefab;
		objectImage.sprite = objectSprite;
		transform.name = $"Button_{objectName}";
	}

	private void SwitchToObject()
	{
		EditorHUDManager.Instance.SwitchCurrentObject(id);
		if (isNone || isDelete)
			ObjectPlacer.Instance.SetCurrentObject(null);
		else
			ObjectPlacer.Instance.SetCurrentObject(objectPrefab);

		ObjectPlacer.Instance.isDelete = isDelete;
	}

	public void ChangeSelectedState(bool isSelected)
	{
		outline.SetActive(isSelected);
	}
}
