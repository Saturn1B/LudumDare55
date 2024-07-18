using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scaleable : ObjectMouseDrag
{
	protected override void Start()
	{
		ObjectRegister.Instance.scaleableObjects.Add(this);
		base.Start();
	}

	protected override void ObjectModification(Vector3 vectorDirection, int vectorDir)
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
			Vector3 tempScale = originaleScale + new Vector3(vectorDirection.x * mouseDelta.x * vectorDir * camRotx, vectorDirection.y * mouseDelta.y * vectorDir, vectorDirection.z * mouseDelta.x * -vectorDir * camRotz);

			if (tempScale.x < 1 || tempScale.y < 1 || tempScale.z < 1) return;

			transform.localScale = tempScale;
			Vector3 newScale = transform.localScale;
			transform.position += new Vector3((newScale.x / 2) - (originaleScale.x / 2), (newScale.y / 2) - (originaleScale.y / 2), (newScale.z / 2) - (originaleScale.z / 2)) * vectorDir;
		}
	}
}
