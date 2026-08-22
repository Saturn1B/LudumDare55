using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionGizmo : GizmoControl
{
	Vector3 startPosition;
	[SerializeField] GameObject commandAffectedObject;

	//protected override void ObjectModification(Vector3 axis)
	//{
	//	int vectorDir = 0;
	//	int usedMouseDelta = 1;

	//	if (mouseDelta.x == 1 || mouseDelta.y == 1)
	//		usedMouseDelta = 1;
	//	else if (mouseDelta.x == -1 || mouseDelta.y == -1)
	//		usedMouseDelta = -1;
	//	else
	//		usedMouseDelta = 0;

	//	int camRotx = 1;
	//	int camRotz = 1;
	//	if (editorCam.parentTransform.transform.eulerAngles.y < 270 && editorCam.parentTransform.transform.eulerAngles.y > 90)
	//		camRotx = -1;
	//	if (editorCam.parentTransform.transform.eulerAngles.y > 180)
	//		camRotz = -1;

	//	if (axis == Vector3.up) vectorDir = 1;
	//	else if (axis == Vector3.right) vectorDir = 1;
	//	else if (axis == Vector3.forward) vectorDir = -1;

	//	if (Input.GetMouseButton(0))
	//	{
	//		Vector3 originalePosition = affectedObject.transform.position;
	//		Vector3 tempPosition = originalePosition + new Vector3(axis.x * usedMouseDelta * vectorDir * camRotx, axis.y * usedMouseDelta * vectorDir, axis.z * usedMouseDelta * vectorDir * camRotz) * .5f;

	//		affectedObject.transform.position = tempPosition;
	//	}
	//}

	protected override void ObjectModification(Vector3 axis)
	{
		Camera cam = Camera.main;
		if (!cam || affectedObject == null)
			return;

		// Convert the local axis into world direction
		Vector3 worldAxis = transform.TransformDirection(axis);

		// Determine how that axis appears on screen
		Vector3 screenPos = cam.WorldToScreenPoint(affectedObject.transform.position);
		Vector3 screenAxisEnd = cam.WorldToScreenPoint(affectedObject.transform.position + worldAxis);
		Vector2 screenAxisDir = (screenAxisEnd - screenPos).normalized;

		// Track mouse movement since the last frame
		Vector2 rawMouseDelta = (Vector2)Input.mousePosition - initMousePos;

		// Project mouse movement onto the axis direction
		float projectedDelta = Vector2.Dot(rawMouseDelta, screenAxisDir);

		// Define how many screen pixels = one snap step
		float pixelThreshold = 5f; // tweak to taste
		float snapSize = .5f;      // world units per step

		// If we passed a threshold, move object one step
		if (Mathf.Abs(projectedDelta) >= pixelThreshold)
		{
			float direction = Mathf.Sign(projectedDelta);

			// Apply one snap step
			affectedObject.transform.position += worldAxis * direction * snapSize;

			// Reset mouse start position for the next step
			initMousePos = Input.mousePosition;
		}
	}

	protected override void StartObjectModification()
	{
		startPosition = commandAffectedObject.transform.GetChild(0).transform.position;
	}

	protected override void EndObjectModification()
	{
		GameObject target = commandAffectedObject.transform.GetChild(0).gameObject;
		MoveCommand moveCommand = new MoveCommand(target, target.GetComponent<ModifiableObject>().objectId, startPosition, target.transform.position);
		HystoryCommand.Instance.ExecuteCommand(moveCommand);
	}
}
