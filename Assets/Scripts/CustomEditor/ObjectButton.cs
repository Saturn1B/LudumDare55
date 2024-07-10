using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObjectButton : MonoBehaviour
{
	private GameObject objectPrefab;
	[SerializeField] private Image objectImage;

	public void ButtonSetter(string objectName, GameObject objectPrefab, Sprite objectSprite)
	{
		this.objectPrefab = objectPrefab;
		objectImage.sprite = objectSprite;
		transform.name = $"Button_{objectName}";
	}
}
