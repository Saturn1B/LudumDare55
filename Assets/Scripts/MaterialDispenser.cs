using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialDispenser : MonoBehaviour
{
	private bool hasPlayer;
	private Transform playerTransform;

	[SerializeField] private Materials dispenserMaterial;
	[SerializeField, Range(0, 5)] private int availableMaterial;

	public Materials GetMaterials()
	{
		return dispenserMaterial;
	}

	public void AddMaterial(int value)
	{
		availableMaterial += value;
	}

	private void Update()
	{
		if(hasPlayer && Input.GetKeyDown(KeyCode.E) && availableMaterial > 0)
		{
			playerTransform.GetComponentInChildren<CreationGun>().SwitchMaterials(dispenserMaterial);
			availableMaterial--;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			hasPlayer = true;
			playerTransform = other.transform;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			hasPlayer = false;
			playerTransform = null;
		}
	}
}
