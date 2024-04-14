using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : Activator
{
	private bool hasPlayer;
	private bool isPressed;
	private Transform playerTransform;
	[SerializeField] private Animator animator;

	private void Update()
	{
		if (hasPlayer && Input.GetKeyDown(KeyCode.E) && !isPressed)
		{
			animator.Play("Base Layer.ButtonPress", 0, 0);
			PowerUp();
			isPressed = true;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			hasPlayer = true;
			HUDManager.Instance.DiplayIndication();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			hasPlayer = false;
			HUDManager.Instance.HideIndication();
		}
	}
}
