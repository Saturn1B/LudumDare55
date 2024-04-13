using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
	private void OnCollisionEnter(Collision collision)
	{
		if (collision.transform.CompareTag("Player") || collision.transform.CompareTag("Object"))
		{
			collision.transform.GetComponent<Rigidbody>().AddForce(Vector3.up * 50, ForceMode.Impulse);
		}
	}
}
