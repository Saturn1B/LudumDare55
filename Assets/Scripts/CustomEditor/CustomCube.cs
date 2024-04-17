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

public class CustomCube : MonoBehaviour
{
	[SerializeField] private GameObject[] highlightFaces;
	private bool highlighted;
	private FreeEditorCam editorCam;

	Vector2 initMousePos;
	bool mousePressed;
	Vector2 oldDelta;
	Vector2 mouseDelta;

	CurrentFaceDirection faceDirection;

	private void Start()
	{
		editorCam = FindObjectOfType<FreeEditorCam>();
	}

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			initMousePos = Input.mousePosition;
			mousePressed = true;
		}
		else if (Input.GetMouseButtonUp(0))
		{
			mousePressed = false;
		}

		if (mousePressed)
		{
			Vector2 newDelta = new Vector2(-Mathf.RoundToInt(initMousePos.x - Input.mousePosition.x) / 100, -Mathf.RoundToInt(initMousePos.y - Input.mousePosition.y) / 100);
			if (oldDelta != newDelta)
			{
				mouseDelta = newDelta;
				if (faceDirection != null)
					Scale(faceDirection.scaleFace, faceDirection.scaleDir);
			}
			else
			{
				mouseDelta = Vector2.zero;
			}

			oldDelta = newDelta;

			Debug.Log(mouseDelta);

			if (mouseDelta.x >= 1 || mouseDelta.y >= 1 || mouseDelta.x <= -1 || mouseDelta.y <= -1)
			{
				initMousePos = Input.mousePosition;
				mouseDelta = Vector2.zero;
			}
		}

		RaycastHit hit;

		Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * 100, Color.red, 1);

		Vector3 mouse = Input.mousePosition;
		Ray castPoint = Camera.main.ScreenPointToRay(mouse);

		if (Physics.Raycast(castPoint, out hit, 100) && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
		{
			if (hit.transform.gameObject != gameObject)
			{
				return;
			}

			Face face = GetHitFace(hit);

			switch (face)
			{
				case Face.None:
					break;
				case Face.Up:
					highlightFaces[0].SetActive(true);
						faceDirection = new CurrentFaceDirection(Vector3.up, 1);
					break;
				case Face.Down:
					highlightFaces[1].SetActive(true);
						faceDirection = new CurrentFaceDirection(Vector3.up, -1);
					break;
				case Face.East:
					highlightFaces[2].SetActive(true);
						faceDirection = new CurrentFaceDirection(Vector3.right ,1);
					break;
				case Face.West:
					highlightFaces[3].SetActive(true);
						faceDirection = new CurrentFaceDirection(Vector3.right ,-1);
					break;
				case Face.North:
					highlightFaces[4].SetActive(true);
						faceDirection = new CurrentFaceDirection(Vector3.forward, 1);
					break;
				case Face.South:
					highlightFaces[5].SetActive(true);
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

	private void Scale(Vector3 scaleDirection, int scaleDir)
	{
		int camRotx = 1;
		int camRotz = 1;
		if (editorCam.parentTransform.transform.eulerAngles.y < 270 && editorCam.parentTransform.transform.eulerAngles.y > 90)
			camRotx = -1;
		if (editorCam.parentTransform.transform.eulerAngles.y > 180)
			camRotz = -1;

		if (Input.GetMouseButton(0))
		{
			Vector3 originaleScale = transform.localScale;
			Vector3 tempScale = originaleScale + new Vector3(scaleDirection.x * mouseDelta.x * scaleDir * camRotx, scaleDirection.y * mouseDelta.y * scaleDir, scaleDirection.z * mouseDelta.x * -scaleDir * camRotz);

			if (tempScale.x < 1 || tempScale.y < 1 || tempScale.z < 1) return;

			transform.localScale = tempScale;
			Vector3 newScale = transform.localScale;
			transform.position += new Vector3((newScale.x / 2) - (originaleScale.x / 2), (newScale.y / 2) - (originaleScale.y / 2), (newScale.z / 2) - (originaleScale.z / 2)) * scaleDir;

			//RecalculateHighlight(newScale);
		}

		if(Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
		{
			faceDirection = null;
		}
	}

	private void RecalculateHighlight(Vector3 newScale)
	{
		foreach (var highlight in highlightFaces)
		{
			if ((highlight.transform.localEulerAngles.z == 0 && highlight.transform.localEulerAngles.x == 0) || (highlight.transform.localEulerAngles.z == 180 && highlight.transform.localEulerAngles.x == 0))
			{
				highlight.transform.localScale = new Vector3(.1f / newScale.x, .1f / newScale.y, .1f / newScale.z);
			}
			else if ((highlight.transform.localEulerAngles.z == 90 && highlight.transform.localEulerAngles.x == 0) || (highlight.transform.localEulerAngles.z == 270 && highlight.transform.localEulerAngles.x == 0))
			{
				highlight.transform.localScale = new Vector3(.1f / newScale.y, .1f / newScale.x, .1f / newScale.z);
			}
			else if (highlight.transform.localEulerAngles.x == 90 || highlight.transform.localEulerAngles.x == 270)
			{
				highlight.transform.localScale = new Vector3(.1f / newScale.x, .1f / newScale.z, .1f / newScale.y);
			}
		}
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
