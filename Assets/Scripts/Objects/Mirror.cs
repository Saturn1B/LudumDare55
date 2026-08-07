using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror : MonoBehaviour, IReflectiveSurface
{
	[SerializeField] private GameObject laserPrefab;
	private Dictionary<LaserEmitter, GameObject> reflections = new Dictionary<LaserEmitter, GameObject>();

	public void GenerateReflection(LaserEmitter source, Vector3 hitPosition, Vector3 hitNormal, Vector3 incomingDirection, Vector3 direction)
	{
		if (reflections.TryGetValue(source, out GameObject laserObject) && laserObject != null)
		{
			laserObject.transform.position = hitPosition;
			laserObject.transform.rotation = Quaternion.LookRotation(direction);
		}
		else
		{
			GameObject newLaser = Instantiate(laserPrefab, hitPosition, Quaternion.LookRotation(direction), this.transform);
			reflections[source] = newLaser;
		}
	}

	public void RemoveReflection(LaserEmitter source)
	{
		if(reflections.TryGetValue(source, out GameObject laserObject))
		{
			if (laserObject != null) Destroy(laserObject);
			reflections.Remove(source);
		}
	}
}
