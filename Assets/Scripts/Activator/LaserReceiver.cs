using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserReceiver : Activator
{
	[SerializeField] private Animator animator;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Laser"))
		{
			other.GetComponentInParent<Object>().receiver = this;

			PowerUp();
			animator.Play("Base Layer.PoweredUp", 0, 0);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Laser"))
		{
			other.GetComponentInParent<Object>().receiver = null;

			PowerDown();
			animator.Play("Base Layer.Stop", 0, 0);
		}
	}

	public void GlobalPowerDown()
	{
		PowerDown();
		animator.Play("Base Layer.Stop", 0, 0);
	}
}
