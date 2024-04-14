using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Activable
{
	private Vector3 startingPos;
	[SerializeField] private Animator animator;
	private BoxCollider boxCollider;

	private void Start()
	{
		startingPos = transform.position;
		boxCollider = GetComponent<BoxCollider>();
	}

	protected override void DoPowered()
	{
		if(boxCollider.enabled == true)
		{
			animator.Play("Base Layer.Opening", 0, 0);
			boxCollider.enabled = false;
		}
	}

	protected override void DoUnpowered()
	{
		if(boxCollider.enabled == false)
		{
			animator.Play("Base Layer.Closing", 0, 0);
			boxCollider.enabled = true;
		}
	}
}
