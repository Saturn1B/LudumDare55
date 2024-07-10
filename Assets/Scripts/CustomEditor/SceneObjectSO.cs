using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Object", menuName = "ScriptableObjects/SceneObject", order = 1)]
public class SceneObjectSO : ScriptableObject
{
	public string objectName;
	public GameObject objectPrefab;
	public Sprite objectSprite;
}
