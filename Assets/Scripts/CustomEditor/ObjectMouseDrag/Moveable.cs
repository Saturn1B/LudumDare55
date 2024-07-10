using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Moveable : ObjectMouseDrag
{
	private void Awake()
	{
		ObjectRegister.Instance.moveableObjects.Add(this);
		this.enabled = false;
	}

	protected override void ObjectModification(Vector3 vectorDirection, int vectorDir)
	{
		int camRotx = 1;
		int camRotz = 1;
		if (editorCam.parentTransform.transform.eulerAngles.y < 270 && editorCam.parentTransform.transform.eulerAngles.y > 90)
			camRotx = -1;
		if (editorCam.parentTransform.transform.eulerAngles.y > 180)
			camRotz = -1;

		if (vectorDirection == Vector3.up) vectorDir = 1;
		else if (vectorDirection == Vector3.right) vectorDir = 1;
		else if (vectorDirection == Vector3.forward) vectorDir = -1;

		if (Input.GetMouseButton(0))
		{
			Vector3 originalePosition = transform.position;
			Vector3 tempPosition = originalePosition + new Vector3(vectorDirection.x * mouseDelta.x * vectorDir * camRotx, vectorDirection.y * mouseDelta.y * vectorDir, vectorDirection.z * mouseDelta.x * vectorDir * camRotz);

			transform.position = tempPosition;
		}
	}
}
