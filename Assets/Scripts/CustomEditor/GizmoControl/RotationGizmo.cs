using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationGizmo : GizmoControl
{
    public bool allowXRot, allowYRot, allowZRot;

	private GameObject objectTransform;

	Quaternion startRotation;

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

	protected override void ObjectModification(Vector3 axis)
	{
		int rotPower = 90;
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
			Quaternion originaleRotation = objectTransform.transform.rotation;

			Debug.Log(axis);

			if (allowXRot && axis.x == 1)
			{
				//Quaternion tempRotation = originaleRotation * Quaternion.AngleAxis(axis.x * usedMouseDelta * rotPower * camRotx, Vector3.right);
				//objectTransform.transform.rotation = tempRotation;
				objectTransform.transform.Rotate(Vector3.right, axis.x * usedMouseDelta * rotPower * camRotx, Space.World);
			}
			if (allowYRot && axis.y == 1)
			{
				//Quaternion tempRotation = originaleRotation * Quaternion.Euler(0, axis.y * usedMouseDelta * -rotPower, 0);
				//objectTransform.transform.rotation = tempRotation;
				objectTransform.transform.Rotate(Vector3.up, axis.y * usedMouseDelta * -rotPower, Space.World);
			}
			if (allowZRot && axis.z == 1)
			{
				//	Quaternion tempRotation = originaleRotation * Quaternion.AngleAxis(axis.z * usedMouseDelta * rotPower * camRotz, Vector3.forward);
				//	objectTransform.transform.rotation = tempRotation;
				objectTransform.transform.Rotate(Vector3.forward, axis.z * usedMouseDelta * rotPower * camRotz, Space.World);
			}
		}
	}

	protected override void StartObjectModification()
	{
		startRotation = affectedObject.transform.GetChild(0).transform.rotation;
	}

	protected override void EndObjectModification()
	{
		GameObject target = affectedObject.transform.GetChild(0).gameObject;
		RotateCommand rotateCommand = new RotateCommand(target, target.GetComponent<ModifiableObject>().objectId, startRotation, target.transform.rotation);
		HystoryCommand.Instance.ExecuteCommand(rotateCommand);
	}
}
