using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belt : Activable
{
	private bool moveBelt;

	protected override void DoPowered()
	{
		moveBelt = true;
	}

	protected override void DoUnpowered()
	{
		moveBelt = false;
	}


	private void OnCollisionStay(Collision collision)
	{
		if (moveBelt)
		{
			if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("Object"))
			{
				collision.rigidbody.AddForce(transform.forward * 20, ForceMode.Force);
			}
		}
	}
}
