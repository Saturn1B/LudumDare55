using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionGizmo : GizmoControl
{
	protected override void ObjectModification(Vector3 axis)
	{
		int vectorDir = 0;
		int usedMouseDelta = 1;

		if (mouseDelta.x == 1 || mouseDelta.y == 1)
			usedMouseDelta = 1;
		else if (mouseDelta.x == -1 || mouseDelta.y == -1)
			usedMouseDelta = -1;
		else
			usedMouseDelta = 0;

		int camRotx = 1;
		int camRotz = 1;
		if (editorCam.parentTransform.transform.eulerAngles.y < 270 && editorCam.parentTransform.transform.eulerAngles.y > 90)
			camRotx = -1;
		if (editorCam.parentTransform.transform.eulerAngles.y > 180)
			camRotz = -1;

		if (axis == Vector3.up) vectorDir = 1;
		else if (axis == Vector3.right) vectorDir = 1;
		else if (axis == Vector3.forward) vectorDir = -1;

		if (Input.GetMouseButton(0))
		{
			Vector3 originalePosition = affectedObject.transform.position;
			Vector3 tempPosition = originalePosition + new Vector3(axis.x * usedMouseDelta * vectorDir * camRotx, axis.y * usedMouseDelta * vectorDir, axis.z * usedMouseDelta * vectorDir * camRotz) * .5f;

			affectedObject.transform.position = tempPosition;
		}
	}
}
