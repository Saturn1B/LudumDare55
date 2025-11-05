using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleGizmo : GizmoControl
{
	[SerializeField] Transform gizmoControl;

	private GameObject objectTransform;

	Vector3 startScale;
	Vector3 startPosition;

	private void OnEnable()
	{
		RefreshGizmo();
	}

	public void RefreshGizmo()
	{
		objectTransform = affectedObject.transform.GetChild(0).gameObject;
	}

	//protected override void ObjectModification(Vector3 axis)
	//{
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

	//	if (Input.GetMouseButton(0))
	//	{
	//		Vector3 originaleScale = objectTransform.transform.lossyScale;
	//		Vector3 tempScale = originaleScale + new Vector3(axis.x * usedMouseDelta * camRotx, axis.y * usedMouseDelta, axis.z * usedMouseDelta * -1 * camRotz) * isInverted;

	//		if (tempScale.x < 1 || tempScale.y < 1 || tempScale.z < 1) return;

	//		objectTransform.transform.localScale = tempScale;
	//		Vector3 newScale = objectTransform.transform.lossyScale;

	//		if(!scaleInPlace)
	//		{
	//			objectTransform.transform.position += new Vector3((newScale.x / 2) - (originaleScale.x / 2), (newScale.y / 2) - (originaleScale.y / 2), (newScale.z / 2) - (originaleScale.z / 2)) * isInverted;

	//			Vector3 tempPos = objectTransform.transform.localPosition;
	//			gizmoControl.position = gizmoControl.position + tempPos;
	//			objectTransform.transform.localPosition = Vector3.zero;
	//		}
	//	}
	//}

	protected override void ObjectModification(Vector3 axis)
	{
		Camera cam = Camera.main;
		if (cam == null)
			return;

		// World axis in world space
		Vector3 worldAxis = objectTransform.transform.TransformDirection(axis);

		// Screen-space positions
		Vector3 screenPos = cam.WorldToScreenPoint(objectTransform.transform.position);
		Vector3 screenAxisEnd = cam.WorldToScreenPoint(objectTransform.transform.position + worldAxis);
		Vector2 screenAxisDir = (screenAxisEnd - screenPos).normalized;

		// Raw mouse movement
		Vector2 rawMouseDelta = (Vector2)Input.mousePosition - initMousePos;

		// Project mouse delta onto axis
		float projectedDelta = Vector2.Dot(rawMouseDelta, screenAxisDir);

		// Threshold in pixels per scaling step
		float pixelThreshold = 5f; // adjust as needed
		float snapSize = 1f;      // scaling step per iteration

		if (Mathf.Abs(projectedDelta) >= pixelThreshold)
		{
			float direction = Mathf.Sign(projectedDelta);

			// Apply scaling along the selected axis
			Vector3 originalScale = objectTransform.transform.localScale;
			Vector3 tempScale = originalScale + worldAxis * direction * snapSize * isInverted;

			// Prevent negative or too small scale
			if (tempScale.x < 1f) tempScale.x = 1f;
			if (tempScale.y < 1f) tempScale.y = 1f;
			if (tempScale.z < 1f) tempScale.z = 1f;

			objectTransform.transform.localScale = tempScale;

			// If not scaling in place, adjust position to maintain pivot
			if (!scaleInPlace)
			{
				Vector3 deltaScale = tempScale - originalScale;
				objectTransform.transform.position += Vector3.Scale(deltaScale, axis) * 0.5f * isInverted;
				gizmoControl.position = gizmoControl.position + objectTransform.transform.localPosition;
				objectTransform.transform.localPosition = Vector3.zero;
			}

			// Reset initMousePos for next discrete step
			initMousePos = Input.mousePosition;
		}
	}

	protected override void StartObjectModification()
	{
		startScale = affectedObject.transform.GetChild(0).transform.localScale;
		startPosition = affectedObject.transform.GetChild(0).transform.position;
	}

	protected override void EndObjectModification()
	{
		GameObject target = affectedObject.transform.GetChild(0).gameObject;
		ScaleCommand scaleCommand = new ScaleCommand(target, target.GetComponent<ModifiableObject>().objectId,
			startScale, target.transform.localScale,
			startPosition, target.transform.position);
		HystoryCommand.Instance.ExecuteCommand(scaleCommand);
	}
}
