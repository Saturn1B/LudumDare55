using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : Activator, IInteractable
{
	private bool isPressed;
	[SerializeField] private Animator animator;

	public void Interact(Transform user)
	{
		if (isPressed) return;

		animator.Play("Base Layer.ButtonPress", 0, 0);
		PowerUp();
		isPressed = true;
	}

	public void EndInteraction()
	{
	}

	public bool CanInteract() => !isPressed;
}
