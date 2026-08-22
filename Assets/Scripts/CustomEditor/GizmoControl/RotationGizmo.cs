using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationGizmo : GizmoControl
{
    public bool allowXRot, allowYRot, allowZRot;

	private GameObject objectTransform;

	Quaternion startRotation;
	Vector3 startScale;

	private void OnEnable()
	{
		RefreshGizmo();
	}

	public void RefreshGizmo()
	{
		objectTransform = affectedObject.transform.GetChild(0).gameObject;

		if (allowXRot) xControl[0].gameObject.SetActive(true);
		else xControl[0].gameObject.SetActive(false);

		if (allowYRot) yControl[0].gameObject.SetActive(true);
		else yControl[0].gameObject.SetActive(false);

		if (allowZRot) zControl[0].gameObject.SetActive(true);
		else zControl[0].gameObject.SetActive(false);
	}

	//protected override void ObjectModification(Vector3 axis)
	//{
	//	int rotPower = 90;
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
	//		Quaternion originaleRotation = objectTransform.transform.rotation;

	//		Debug.Log(axis);

	//		if (allowXRot && axis.x == 1)
	//		{
	//			if (!objectTransform.GetComponent<ReCalcCubeTexture>())
	//				objectTransform.transform.Rotate(Vector3.right, axis.x * usedMouseDelta * rotPower * camRotx, Space.World);
	//			else
	//				objectTransform.transform.localScale = new Vector3(objectTransform.transform.localScale.x, objectTransform.transform.localScale.z, objectTransform.transform.localScale.y);
	//		}
	//		if (allowYRot && axis.y == 1)
	//		{
	//			if (!objectTransform.GetComponent<ReCalcCubeTexture>())
	//				objectTransform.transform.Rotate(Vector3.up, axis.y * usedMouseDelta * -rotPower, Space.World);
	//			else
	//				objectTransform.transform.localScale = new Vector3(objectTransform.transform.localScale.z, objectTransform.transform.localScale.y, objectTransform.transform.localScale.x);
	//		}
	//		if (allowZRot && axis.z == 1)
	//		{
	//			if (!objectTransform.GetComponent<ReCalcCubeTexture>())
	//				objectTransform.transform.Rotate(Vector3.forward, axis.z * usedMouseDelta * rotPower * camRotz, Space.World);
	//			else
	//				objectTransform.transform.localScale = new Vector3(objectTransform.transform.localScale.y, objectTransform.transform.localScale.x, objectTransform.transform.localScale.z);
	//		}
	//	}
	//}

	protected override void ObjectModification(Vector3 axis)
	{
		Camera cam = Camera.main;
		if (!cam || objectTransform == null)
			return;

		// Convert local axis to world direction
		Vector3 worldAxis = transform.TransformDirection(axis);

		// Determine screen-space direction of the rotation axis
		Vector3 screenPos = cam.WorldToScreenPoint(objectTransform.transform.position);
		Vector3 screenAxisEnd = cam.WorldToScreenPoint(objectTransform.transform.position + worldAxis);
		Vector2 screenAxisDir = (screenAxisEnd - screenPos).normalized;

		// --- Fix: use horizontal motion for Y rotation ---
		if (axis == Vector3.up)
		{
			Vector3 camRight = cam.transform.right;
			Vector3 screenCamRightEnd = cam.WorldToScreenPoint(objectTransform.transform.position + camRight);
			screenAxisDir = (screenCamRightEnd - screenPos).normalized;
		}

		// Get mouse delta since last step
		Vector2 rawMouseDelta = (Vector2)Input.mousePosition - initMousePos;
		float projectedDelta = Vector2.Dot(rawMouseDelta, screenAxisDir);

		// Threshold and step settings
		float pixelThreshold = 10f; // pixels of mouse move per rotation step
		float snapAngle = 90f;      // degrees per step (set to 90 if you prefer)

		if (Mathf.Abs(projectedDelta) >= pixelThreshold)
		{
			float direction = Mathf.Sign(projectedDelta);

			// Rotate only on allowed axes
			if (allowXRot && axis.x == 1)
			{
				if (!objectTransform.GetComponent<ReCalcCubeTexture>())
					objectTransform.transform.Rotate(worldAxis, -direction * snapAngle, Space.World);
				else
					objectTransform.transform.localScale = new Vector3(
						objectTransform.transform.localScale.x,
						objectTransform.transform.localScale.z,
						objectTransform.transform.localScale.y);
			}

			if (allowYRot && axis.y == 1)
			{
				if (!objectTransform.GetComponent<ReCalcCubeTexture>())
					objectTransform.transform.Rotate(worldAxis, -direction * snapAngle, Space.World);
				else
					objectTransform.transform.localScale = new Vector3(
						objectTransform.transform.localScale.z,
						objectTransform.transform.localScale.y,
						objectTransform.transform.localScale.x);
			}

			if (allowZRot && axis.z == 1)
			{
				if (!objectTransform.GetComponent<ReCalcCubeTexture>())
					objectTransform.transform.Rotate(worldAxis, -direction * snapAngle, Space.World);
				else
					objectTransform.transform.localScale = new Vector3(
						objectTransform.transform.localScale.y,
						objectTransform.transform.localScale.x,
						objectTransform.transform.localScale.z);
			}

			// Reset reference mouse position for next snap step
			initMousePos = Input.mousePosition;
		}
	}

	protected override void StartObjectModification()
	{
		if (!objectTransform.GetComponent<ReCalcCubeTexture>())
			startRotation = affectedObject.transform.GetChild(0).transform.rotation;
		else
			startScale = affectedObject.transform.GetChild(0).transform.localScale;
	}

	protected override void EndObjectModification()
	{
		GameObject target = affectedObject.transform.GetChild(0).gameObject;
		if (!objectTransform.GetComponent<ReCalcCubeTexture>())
		{
			RotateCommand rotateCommand = new RotateCommand(target, target.GetComponent<ModifiableObject>().objectId, startRotation, target.transform.rotation);
			HystoryCommand.Instance.ExecuteCommand(rotateCommand);
		}
		else
		{
			ScaleCommand scaleCommand = new ScaleCommand(target, target.GetComponent<ModifiableObject>().objectId,
				startScale, target.transform.localScale,
				target.transform.position, target.transform.position, true);
			HystoryCommand.Instance.ExecuteCommand(scaleCommand);
		}
	}
}
