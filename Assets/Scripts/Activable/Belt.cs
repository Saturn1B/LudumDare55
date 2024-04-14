using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belt : Activable
{
	private bool moveBelt;
	[SerializeField] Material on, off;
	[SerializeField] GameObject[] arrows;

	protected override void DoPowered()
	{
		if(moveBelt == false)
		{
			moveBelt = true;
			foreach (var arrow in arrows)
			{
				arrow.GetComponent<MeshRenderer>().material = on;
			}
		}
	}

	protected override void DoUnpowered()
	{
		if(moveBelt == true)
		{
			moveBelt = false;
			foreach (var arrow in arrows)
			{
				arrow.GetComponent<MeshRenderer>().material = off;
			}
		}
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
