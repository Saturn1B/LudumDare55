using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

	[SerializeField] private GameObject indicationText;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	public void DiplayIndication()
	{
		indicationText.SetActive(true);
	}

	public void HideIndication()
	{
		indicationText.SetActive(false);
	}
}
