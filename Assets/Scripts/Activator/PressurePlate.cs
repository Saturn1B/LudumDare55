using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Activator, IPlayerDetector
{
	private int objectCounter;
	[SerializeField] private Animator animator;

	public void RemoveOneObject()
	{
		objectCounter--;
		if (objectCounter == 0)
		{
			animator.Play("Base Layer.PlateUp", 0, 0);
			PowerDown();
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.CompareTag("Object"))
		{
			collision.transform.GetComponent<Object>().plate = this;

			CollisionDetect(true);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.transform.CompareTag("Object"))
		{
			collision.transform.GetComponent<Object>().plate = null;

			CollisionDetect(false);
		}
	}

	private void CollisionDetect(bool entering)
	{
		if (entering)
		{
			if (objectCounter == 0)
			{
				animator.Play("Base Layer.PlateDown", 0, 0);
				PowerUp();
			}
			objectCounter++;
		}
		else
		{
			objectCounter--;
			if (objectCounter == 0)
			{
				animator.Play("Base Layer.PlateUp", 0, 0);
				PowerDown();
			}
		}
	}

	public void StartDetection(Transform player)
	{
		CollisionDetect(true);
	}

	public void EndDetection()
	{
		CollisionDetect(false);
	}
}
