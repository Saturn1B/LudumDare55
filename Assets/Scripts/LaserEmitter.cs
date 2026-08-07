using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserEmitter : MonoBehaviour
{
	[SerializeField] private Transform laser;
	[SerializeField] private LayerMask layer;
	private IReflectiveSurface currentReflector;

	private void Update()
	{
		RaycastHit hit;
		if (Physics.Raycast(transform.position, transform.forward, out hit, 100, ~layer))
		{
			laser.localScale = new Vector3(.02f, .02f, hit.distance);
			laser.localPosition = new Vector3(0, 0, hit.distance / 2);

			if (hit.transform.TryGetComponent(out IReflectiveSurface reflector))
			{
				if(currentReflector != null && currentReflector != reflector)
				{
					currentReflector.RemoveReflection(this);
				}

				currentReflector = reflector;
				Vector3 reflectedDir = Vector3.Reflect(transform.forward, hit.normal);
				reflector.GenerateReflection(this, hit.point, hit.normal, transform.forward, reflectedDir);
			}
			else
			{
				if (currentReflector != null)
				{
					currentReflector.RemoveReflection(this);
					currentReflector = null;
				}
			}
		}
		else
		{
			if (currentReflector != null) currentReflector.RemoveReflection(this);

			laser.localScale = new Vector3(.02f, .02f, 100);
			laser.localPosition = new Vector3(0, 0, 100 / 2);
		}
	}

	private void OnDestroy()
	{
		if(currentReflector != null)
		{
			currentReflector.RemoveReflection(this);
		}
	}
}
