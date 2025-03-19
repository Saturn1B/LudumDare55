using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SceneObjectType
{
	WALLS = 0,
	ACTIVATORS = 1,
	ACTIVABLES = 2,
	OTHERS = 3,
	HIDE = -1,
	ALL = 100
}

[CreateAssetMenu(fileName = "Object", menuName = "ScriptableObjects/SceneObject", order = 1)]
public class SceneObjectSO : ScriptableObject
{
	public string objectName;
	public GameObject objectPrefab;
	public Sprite objectSprite;
	public SceneObjectType objectType;
}
