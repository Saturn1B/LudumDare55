using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Axis
{
	NONE,
	X,
	Y,
	Z
}

public class GizmoControl : MonoBehaviour
{
	protected FreeEditorCam editorCam;

	Vector2 initMousePos;
	bool mousePressed;
	Vector2 oldDelta;
	protected Vector2 mouseDelta;
	protected int isInverted;

	public bool wasSelected;
	public bool isSelected;

	public Transform[] xControl, yControl, zControl;
	public Transform[] normalControl, invertedControl;
	public LayerMask gizmoLayer;
	public GameObject affectedObject;
	private CurrentAxis currentAxis;

	protected virtual void Start()
	{
		editorCam = FindObjectOfType<FreeEditorCam>();
	}

	private void Update()
	{
		if (EditorHUDManager.Instance.isPaused) return;

		if (Input.GetMouseButtonUp(0))
		{
			mousePressed = false;
			currentAxis = null;

			EndObjectModification();
		}

		if (mousePressed)
		{

			Vector2 newDelta = new Vector2(-Mathf.RoundToInt(initMousePos.x - Input.mousePosition.x) / 100, -Mathf.RoundToInt(initMousePos.y - Input.mousePosition.y) / 100);
			if (oldDelta != newDelta)
			{
				mouseDelta = newDelta;
				if (currentAxis != null)
					ObjectModification(currentAxis.axis);
			}
			else
			{
				mouseDelta = Vector2.zero;
			}

			oldDelta = newDelta;

			if (mouseDelta.x >= 1 || mouseDelta.y >= 1 || mouseDelta.x <= -1 || mouseDelta.y <= -1)
			{
				initMousePos = Input.mousePosition;
				mouseDelta = Vector2.zero;
			}
		}

		RaycastHit hit;
		Vector3 mouse = Input.mousePosition;
		Ray castPoint = Camera.main.ScreenPointToRay(mouse);

		if (Physics.Raycast(castPoint, out hit, 100, gizmoLayer) && (Input.GetMouseButtonDown(0)) /*&& wasSelected == isSelected*/)
		{
			Debug.Log("shoot raycast");

			if (GetHitAxis(hit) == Axis.NONE)
			{
				Debug.Log("nope");

				return;
			}

			if (Input.GetMouseButtonDown(0))
			{
				initMousePos = Input.mousePosition;
				mousePressed = true;
			}

			Axis _axis = GetHitAxis(hit);

			switch (_axis)
			{
				case Axis.NONE:
					break;
				case Axis.X:
					currentAxis = new CurrentAxis(Vector3.right);
					break;
				case Axis.Y:
					currentAxis = new CurrentAxis(Vector3.up);
					break;
				case Axis.Z:
					currentAxis = new CurrentAxis(Vector3.forward);
					break;
				default:
					break;
			}
		}
	}

	private Axis GetHitAxis(RaycastHit hit)
	{
		foreach (Transform control in normalControl)
		{
			if (hit.transform == control)
				isInverted = 1;
		}

		foreach (Transform control in invertedControl)
		{
			if (hit.transform == control)
				isInverted = -1;
		}

		foreach (Transform control in xControl)
		{
			if (hit.transform == control)
				return Axis.X;
		}

		foreach (Transform control in yControl)
		{
			if (hit.transform == control)
				return Axis.Y;
		}

		foreach (Transform control in zControl)
		{
			if (hit.transform == control)
				return Axis.Z;
		}

		return Axis.NONE;
	}

	protected virtual void ObjectModification(Vector3 vectorDirection)
	{

	}

	protected virtual void EndObjectModification()
	{

	}
}

[System.Serializable]
public class CurrentAxis
{
	public Vector3 axis;

	public CurrentAxis(Vector3 axis)
	{
		this.axis = axis;
	}
}
