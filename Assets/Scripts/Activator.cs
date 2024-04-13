using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activator : MonoBehaviour
{
	[SerializeField] Activable linkedActivable;

	protected virtual void PowerUp()
	{
		linkedActivable.isPowered = true;
	}

	protected virtual void PowerDown()
	{
		linkedActivable.isPowered = false;
	}
}
