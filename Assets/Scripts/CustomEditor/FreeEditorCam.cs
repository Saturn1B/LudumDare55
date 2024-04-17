using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeEditorCam : MonoBehaviour
{
	[SerializeField] private float movementSpeed;
	[SerializeField] private float secondaryMovementSpeed;
	[SerializeField] private float rotationSpeed;
	[SerializeField] private float scrollSpeed;

	public Transform parentTransform;

	private bool isRightMouseButtonPressed;

	private void Update()
	{
		if (Input.GetMouseButtonDown(1))
			isRightMouseButtonPressed = true;
		else if (Input.GetMouseButtonUp(1))
			isRightMouseButtonPressed = false;

		if (isRightMouseButtonPressed)
		{
			float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
			float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

			parentTransform.Rotate(Vector3.up, mouseX, Space.World);
			transform.Rotate(Vector3.left, mouseY, Space.Self);

			float horizontal = Input.GetAxis("Horizontal") * movementSpeed * Time.deltaTime;
			float vertical = Input.GetAxis("Vertical") * movementSpeed * Time.deltaTime;

			parentTransform.Translate(transform.forward * vertical, Space.World);
			parentTransform.Translate(transform.right * horizontal, Space.World);

			float upDown = 0f;
			if (Input.GetKey(KeyCode.Space))
				upDown += 1;
			if (Input.GetKey(KeyCode.LeftShift))
				upDown -= 1;

			parentTransform.Translate(Vector3.up * upDown * movementSpeed * Time.deltaTime, Space.World);
		}
		else
		{
			if (Input.GetMouseButton(2))
			{
				float mouseX = -Input.GetAxis("Mouse X") * secondaryMovementSpeed * Time.deltaTime;
				float mouseY = -Input.GetAxis("Mouse Y") * secondaryMovementSpeed * Time.deltaTime;

				parentTransform.Translate(new Vector3(mouseX, mouseY, 0));
			}

			float scroll = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
			parentTransform.Translate(transform.forward * scroll, Space.World);
		}
	}
}
