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
	}

	public void ChangeSelectedState(bool isSelected)
	{
		outline.SetActive(isSelected);
	}
}
