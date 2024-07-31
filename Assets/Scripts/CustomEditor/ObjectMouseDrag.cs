using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Face
{
	None,
	Up,
	Down,
	East,
	West,
	North,
	South
}

public class ObjectMouseDrag : MonoBehaviour
{
	protected FreeEditorCam editorCam;

	Vector2 initMousePos;
	bool mousePressed;
	Vector2 oldDelta;
	protected Vector2 mouseDelta;

	CurrentFaceDirection faceDirection;

	public bool wasSelected;
	public bool isSelected;

	protected virtual void Start()
	{
		editorCam = FindObjectOfType<FreeEditorCam>();
		ObjectRegister.Instance.RefreshMode();
	}

	private void Update()
	{
		if (Input.GetMouseButtonUp(0))
		{
			mousePressed = false;
			faceDirection = null;
		}

		if (mousePressed)
		{

			Vector2 newDelta = new Vector2(-Mathf.RoundToInt(initMousePos.x - Input.mousePosition.x) / 100, -Mathf.RoundToInt(initMousePos.y - Input.mousePosition.y) / 100);
			if (oldDelta != newDelta)
			{
				mouseDelta = newDelta;
				if (faceDirection != null)
					ObjectModification(faceDirection.scaleFace, faceDirection.scaleDir);
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

		if (Physics.Raycast(castPoint, out hit, 100) && (Input.GetMouseButtonDown(0)) && wasSelected == isSelected)
		{
			Debug.Log("test2");
			if (hit.transform.gameObject != gameObject)
			{
				return;
			}

			if (Input.GetMouseButtonDown(0))
			{
				initMousePos = Input.mousePosition;
				mousePressed = true;
			}

			Face face = GetHitFace(hit);

			switch (face)
			{
				case Face.None:
					break;
				case Face.Up:
					faceDirection = new CurrentFaceDirection(Vector3.up, 1);
					break;
				case Face.Down:
					faceDirection = new CurrentFaceDirection(Vector3.up, -1);
					break;
				case Face.East:
					faceDirection = new CurrentFaceDirection(Vector3.right, 1);
					break;
				case Face.West:
					faceDirection = new CurrentFaceDirection(Vector3.right, -1);
					break;
				case Face.North:
					faceDirection = new CurrentFaceDirection(Vector3.forward, 1);
					break;
				case Face.South:
					faceDirection = new CurrentFaceDirection(Vector3.forward, -1);
					break;
				default:
					break;
			}
		}
	}

	private Face GetHitFace(RaycastHit hit)
	{
		Vector3 incomingVec = hit.normal - Vector3.up;

		if (incomingVec == new Vector3(0, -1, -1))
			return Face.South;

		if (incomingVec == new Vector3(0, -1, 1))
			return Face.North;

		if (incomingVec == new Vector3(0, 0, 0))
			return Face.Up;

		if (incomingVec == new Vector3(0, -2, 0))
			return Face.Down;

		if (incomingVec == new Vector3(-1, -1, 0))
			return Face.West;

		if (incomingVec == new Vector3(1, -1, 0))
			return Face.East;

		return Face.None;
	}

	protected virtual void ObjectModification(Vector3 vectorDirection, int vectorDir)
	{

	}
}

[System.Serializable]
public class CurrentFaceDirection
{
	public Vector3 scaleFace;
	public int scaleDir;

	public CurrentFaceDirection(Vector3 scaleFace, int scaleDir)
	{
		this.scaleFace = scaleFace;
		this.scaleDir = scaleDir;
	}
}
