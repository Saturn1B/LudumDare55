using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModifiableObject : MonoBehaviour
{
	private static HashSet<string> usedIds = new HashSet<string>();

	public bool isGroundOrWall;
	public bool isStuckToWall;
	public bool canTranslate, canScale, canRotate;
	public bool X, Y, Z;

	public string objectId { get; private set; }

	public GameObject parentPrefab;

	public bool hasChildObjects;
	public ModifiableObject[] childObjects;

	public bool hasParent;
	public ModifiableObject parentObject;

	private void Awake()
	{
		objectId = GenerateUniqueID();
	}

	private string GenerateUniqueID()
	{
		string id;
		do
		{
			id = System.Guid.NewGuid().ToString();
		}
		while (usedIds.Contains(id));

		usedIds.Add(id);
		return id;
	}

	public void OverrideID(string preferedId)
	{
		objectId = preferedId;
	}
}
