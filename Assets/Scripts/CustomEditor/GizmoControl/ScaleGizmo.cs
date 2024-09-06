using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleGizmo : GizmoControl
{
	[SerializeField] Transform gizmoControl;

	private GameObject objectTransform;

	private void OnEnable()
	{
		objectTransform = affectedObject.transform.GetChild(0).gameObject;
	}

	protected override void ObjectModification(Vector3 axis)
	{
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

		if (Input.GetMouseButton(0))
		{
			Vector3 originaleScale = objectTransform.transform.lossyScale;
			Vector3 tempScale = originaleScale + new Vector3(axis.x * usedMouseDelta * camRotx, axis.y * usedMouseDelta, axis.z * usedMouseDelta * -1 * camRotz);

			if (tempScale.x < 1 || tempScale.y < 1 || tempScale.z < 1) return;

			objectTransform.transform.localScale = tempScale;
			Vector3 newScale = objectTransform.transform.lossyScale;
			objectTransform.transform.position += new Vector3((newScale.x / 2) - (originaleScale.x / 2), (newScale.y / 2) - (originaleScale.y / 2), (newScale.z / 2) - (originaleScale.z / 2));

			Vector3 tempPos = objectTransform.transform.localPosition;
			gizmoControl.position = gizmoControl.position + tempPos;
			objectTransform.transform.localPosition = Vector3.zero;
		}
	}
}
