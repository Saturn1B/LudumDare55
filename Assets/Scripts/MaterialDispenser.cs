using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialDispenser : MonoBehaviour, IInteractable
{
	[SerializeField] private Materials dispenserMaterial;
	[SerializeField, Range(0, 5)] private int availableMaterial;

	[SerializeField] private GameObject[] barGraph;
	[SerializeField] private GameObject center;

	[SerializeField] private Material[] matOn;
	[SerializeField] private Material[] matOff;

	public Materials GetMaterials()
	{
		return dispenserMaterial;
	}

	public void AddMaterial(int value)
	{
		availableMaterial += value;
		UpdateGraph();
	}

	private void Start()
	{
		UpdateGraph();
	}

	private void UpdateGraph()
	{
		for (int i = 0; i < barGraph.Length; i++)
		{
			barGraph[i].GetComponent<MeshRenderer>().material = i + 1 <= availableMaterial ? matOn[(int)dispenserMaterial - 1] : matOff[(int)dispenserMaterial - 1];
		}

		center.GetComponent<MeshRenderer>().material = availableMaterial <= 0 ? matOff[(int)dispenserMaterial - 1] : matOn[(int)dispenserMaterial - 1];
	}

	public void Interact(Transform user)
	{
		if (availableMaterial > 0)
		{
			if (!user.GetComponentInChildren<CreationGun>().SwitchMaterials(dispenserMaterial)) return;
			availableMaterial--;

			UpdateGraph();
		}
	}

	public void EndInteraction()
	{
	}

	public bool CanInteract()
	{
		return availableMaterial > 0;
	}
}
