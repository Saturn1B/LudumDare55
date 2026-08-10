using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

	[SerializeField] private GameObject indicationText;

	private bool isIndicationOn;

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
		if (isIndicationOn) return;

		indicationText.SetActive(true);
		isIndicationOn = true;
	}

	public void HideIndication()
	{
		if (!isIndicationOn) return;

		indicationText.SetActive(false);
		isIndicationOn = false;
	}
}
