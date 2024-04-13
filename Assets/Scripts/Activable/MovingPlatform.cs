using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : Activable
{
	[SerializeField] private Transform pointA, pointB;
	[SerializeField] private Platform platform;
	[SerializeField] private float speed;
	public bool reverse;

	protected override void DoPowered()
	{
		var step = speed * Time.deltaTime;

		if (reverse)
		{
			platform.transform.position = Vector3.MoveTowards(platform.transform.position, pointA.position, step);
			foreach (var item in platform.objectOnPlatform)
			{
				item.transform.position = Vector3.MoveTowards(item.transform.position, pointA.position, step);
			}
		}
		else
		{
			platform.transform.position = Vector3.MoveTowards(platform.transform.position, pointB.position, step);
			foreach (var item in platform.objectOnPlatform)
			{
				item.transform.position = Vector3.MoveTowards(item.transform.position, pointB.position, step);
			}
		}
	}
}
