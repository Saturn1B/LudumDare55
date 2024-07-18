using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectRegister : MonoBehaviour
{
	public static ObjectRegister Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	[HideInInspector] public List<Moveable> moveableObjects = new List<Moveable>();
	[HideInInspector] public List<Scaleable> scaleableObjects = new List<Scaleable>();

	private MouseDragMode _mouseDragMode;

	public void SetMoveableMode()
	{
		//Deactivate other component
		foreach (Scaleable scaleable in scaleableObjects)
		{
			scaleable.enabled = false;
		}

		//Activate moveable component
		foreach (Moveable moveable in moveableObjects)
		{
			moveable.enabled = true;
		}

		_mouseDragMode = MouseDragMode.MOVEABLE;
	}

	public void SetScaleableMode()
	{
		//Deactivate other component
		foreach (Moveable moveable in moveableObjects)
		{
			moveable.enabled = false;
		}

		//Activate moveable component
		foreach (Scaleable scaleable in scaleableObjects)
		{
			scaleable.enabled = true;
		}

		_mouseDragMode = MouseDragMode.SCALEABLE;
	}

	public void RefreshMode()
	{
		switch (_mouseDragMode)
		{
			case MouseDragMode.MOVEABLE:
				SetMoveableMode();
				break;
			case MouseDragMode.SCALEABLE:
				SetScaleableMode();
				break;
			case MouseDragMode.PIVOTABLE:
				break;
			default:
				break;
		}
	}
}
