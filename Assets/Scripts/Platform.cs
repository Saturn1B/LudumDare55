using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    [SerializeField] private MovingPlatform platform;

	public List<Transform> objectOnPlatform = new List<Transform>();


	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("PlatformEndPoint"))
		{
			platform.reverse = !platform.reverse;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if(collision.transform.CompareTag("Player") || collision.transform.CompareTag("Object"))
		{
			if (collision.transform.CompareTag("Object"))
			{
				collision.transform.GetComponent<Object>().platform = this;
			}

			objectOnPlatform.Add(collision.transform);
		}
	}

	private void OnCollisionExit(Collision collision)
	{
		if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("Object"))
		{
			if (collision.transform.CompareTag("Object"))
			{
				collision.transform.GetComponent<Object>().platform = null;
			}

			objectOnPlatform.Remove(collision.transform);
		}
	}
}
