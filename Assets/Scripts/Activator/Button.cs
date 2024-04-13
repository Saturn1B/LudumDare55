using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : Activator
{
	private bool hasPlayer;
	private bool isPressed;
	private Transform playerTransform;

	private void Update()
	{
		if (hasPlayer && Input.GetKeyDown(KeyCode.E) && !isPressed)
		{
			StartCoroutine(ButtonPress());
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			hasPlayer = true;
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			hasPlayer = false;
		}
	}

	private IEnumerator ButtonPress()
	{
		PowerUp();
		isPressed = true;
		yield return new WaitForSeconds(2f);
		PowerDown();
		isPressed = false;
	}
}
