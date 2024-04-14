using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : Activator
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
		if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("Object"))
		{
			if (collision.transform.CompareTag("Object"))
			{
				collision.transform.GetComponent<Object>().plate = this;
			}

			if (objectCounter == 0)
			{
				animator.Play("Base Layer.PlateDown", 0, 0);
				PowerUp();
			}
			objectCounter++;
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("Object"))
		{
			if (collision.transform.CompareTag("Object"))
			{
				collision.transform.GetComponent<Object>().plate = null;
			}

			objectCounter--;
			if (objectCounter == 0)
			{
				animator.Play("Base Layer.PlateUp", 0, 0);
				PowerDown();
			}
		}
	}
}
