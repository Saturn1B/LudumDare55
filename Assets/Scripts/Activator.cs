using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
	public List<Activable> linkedActivable = new List<Activable>();

	protected virtual void PowerUp()
	{
		foreach (var activable in linkedActivable)
		{
			activable.isPowered = true;
		}
	}

	protected virtual void PowerDown()
	{
		foreach (var activable in linkedActivable)
		{
			activable.isPowered = false;
		}
	}
}
