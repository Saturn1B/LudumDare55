using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDetect : MonoBehaviour
{
	[SerializeField] private GameObject reflectedLaser;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Laser"))
		{
			reflectedLaser.SetActive(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Laser"))
		{
			reflectedLaser.SetActive(false);
		}
	}
}
