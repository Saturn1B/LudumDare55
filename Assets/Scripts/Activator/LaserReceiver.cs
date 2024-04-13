using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserReceiver : Activator
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Laser"))
		{
			other.GetComponentInParent<Object>().receiver = this;

			PowerUp();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Laser"))
		{
			other.GetComponentInParent<Object>().receiver = null;

			PowerDown();
		}
	}

	public void GlobalPowerDown()
	{
		PowerDown();
	}
}
