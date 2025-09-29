using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    [SerializeField] private Transform cameraTarget;

	private void OnEnable()
	{
		if (cameraTarget == null)
			cameraTarget = FindObjectOfType<FreeEditorCam>().transform;
	}

	void Update()
    {
        transform.LookAt(cameraTarget);
    }
}
