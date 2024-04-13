using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : Activable
{
	private Vector3 startingPos;

	private void Start()
	{
		startingPos = transform.position;
	}

	protected override void DoPowered()
	{
		if(transform.position == startingPos)
			transform.position = startingPos + Vector3.right * 2;
	}

	protected override void DoUnpowered()
	{
		if (transform.position == startingPos + Vector3.right * 2)
			transform.position = startingPos;
	}
}
