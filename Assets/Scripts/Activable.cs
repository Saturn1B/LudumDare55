using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Activable : MonoBehaviour
{
    [SerializeField] protected bool needPower;
    [HideInInspector] public bool isPowered;

	private void Update()
	{
		if (needPower)
		{
			if (isPowered)
			{
				DoPowered();
			}
			else
			{
				DoUnpowered();
			}
		}
		else
		{
			DoPowered();
		}
	}

	protected virtual void DoPowered()
	{

	}

	protected virtual void DoUnpowered()
	{

	}
}
