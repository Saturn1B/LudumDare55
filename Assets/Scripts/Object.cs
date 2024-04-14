using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Object : MonoBehaviour
{
	public Materials objectMaterial;
	public PressurePlate plate;
	public Platform platform;
	public LaserReceiver receiver;

	public Sprite objectImage;

	private void OnDestroy()
	{
		if(plate != null)
		{
			plate.RemoveOneObject();
		}
		if(platform != null)
		{
			platform.objectOnPlatform.Remove(this.transform);
		}
		if (receiver != null)
		{
			receiver.GlobalPowerDown();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if ((other.CompareTag("Kill") || other.CompareTag("Laser")) && objectMaterial == Materials.COMPOSITE)
		{
			MaterialDispenser[] dispensers = FindObjectsOfType<MaterialDispenser>();
			foreach (MaterialDispenser dispenser in dispensers)
			{
				if (dispenser.GetMaterials() == objectMaterial)
				{
					dispenser.AddMaterial(1);
				}
			}

			Destroy(this.gameObject);
		}
	}
}
