using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialDispenser : MonoBehaviour
{
	private bool hasPlayer;
	private Transform playerTransform;

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

	private void Update()
	{
		if(hasPlayer && Input.GetKeyDown(KeyCode.E) && availableMaterial > 0)
		{
			if(!playerTransform.GetComponentInChildren<CreationGun>().SwitchMaterials(dispenserMaterial)) return;
			availableMaterial--;
			if (availableMaterial <= 0)
				HUDManager.Instance.HideIndication();
			UpdateGraph();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			hasPlayer = true;
			playerTransform = other.transform;
			if(availableMaterial > 0)
				HUDManager.Instance.DiplayIndication();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			hasPlayer = false;
			playerTransform = null;
			HUDManager.Instance.HideIndication();
		}
	}
}
