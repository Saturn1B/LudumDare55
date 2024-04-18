using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserDetect : MonoBehaviour
{
	[SerializeField] private GameObject reflectedLaser;
	[SerializeField] private GameObject[] detectionSides;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Laser"))
		{
			bool isAlreadyActive = false;
			foreach (GameObject side in detectionSides)
			{
				if (side.activeSelf)
				{
					isAlreadyActive = true;
					break;
				}
			}

			if(!isAlreadyActive)
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
