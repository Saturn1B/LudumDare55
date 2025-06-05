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
	private bool isLeftMouseButtonPressed;

	public static bool isModifyingObject;

	private void Awake()
	{
		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	private void Update()
	{
		if (EditorHUDManager.Instance.isPaused) return;

		if (Input.GetMouseButtonDown(1))
			isRightMouseButtonPressed = true;
		else if (Input.GetMouseButtonUp(1))
			isRightMouseButtonPressed = false;

		if (Input.GetMouseButtonDown(0))
			isLeftMouseButtonPressed = true;
		else if (Input.GetMouseButtonUp(0))
			isLeftMouseButtonPressed = false;

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
		else if (isLeftMouseButtonPressed)
		{
			if(ObjectSelection.Instance.selected != null && !isModifyingObject)
			{
				Transform selectedObject = ObjectSelection.Instance.selected;

				// Input de la souris
				float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
				float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

				Debug.Log($"MouseX: {Input.GetAxis("Mouse X")}, MouseY: {Input.GetAxis("Mouse Y")}, MousePos: {Input.mousePosition}");

				// Calculer la position actuelle relative à l'objet
				Vector3 offset = transform.position - selectedObject.position;

				// Rotation horizontale (Y) - on fait tourner l'offset autour de l'axe Y
				Quaternion horizontalRotation = Quaternion.AngleAxis(mouseX, Vector3.up);
				offset = horizontalRotation * offset;

				// Rotation verticale (X) - on fait tourner l'offset autour de l'axe right du parent
				Vector3 rightAxis = parentTransform.right;
				Quaternion verticalRotation = Quaternion.AngleAxis(-mouseY, rightAxis);
				offset = verticalRotation * offset;

				// Calculer la nouvelle position
				Vector3 newPosition = selectedObject.position + offset;

				// Mettre à jour la position du parent (qui porte la caméra)
				parentTransform.position = newPosition;

				// Appliquer les rotations aux transforms en respectant les contraintes
				// Parent : seulement rotation Y (rotation relative)
				parentTransform.Rotate(Vector3.up, mouseX, Space.World);

				// Caméra : seulement rotation X (rotation relative)
				transform.Rotate(Vector3.left, mouseY, Space.Self);
			}
		}
		else
		{
			if (Input.GetMouseButton(2))
			{
				float mouseX = -Input.GetAxis("Mouse X") * secondaryMovementSpeed * Time.deltaTime;
				float mouseY = -Input.GetAxis("Mouse Y") * secondaryMovementSpeed * Time.deltaTime;

				parentTransform.Translate(new Vector3(mouseX, mouseY, 0));
			}

			if (!ObjectPlacer.Instance.mouseOverSelecterUI && !ObjectSelection.Instance.mouseOverEditorUI)
			{
				float scroll = Input.GetAxis("Mouse ScrollWheel") * scrollSpeed * Time.deltaTime;
				parentTransform.Translate(transform.forward * scroll, Space.World);
			}
		}
	}

	public void SetCamRotation(Transform target)
	{
		if (target == null) return;

		// Calculer la direction de la caméra vers la cible
		Vector3 direction = target.position - transform.position;

		// Calculer la rotation nécessaire pour regarder la cible
		Quaternion lookRotation = Quaternion.LookRotation(direction);
		Vector3 eulerAngles = lookRotation.eulerAngles;

		// Décomposer la rotation en respectant les contraintes
		// Parent (cameraHolder) : seulement l'axe Y
		parentTransform.rotation = Quaternion.Euler(0, eulerAngles.y, 0);

		// Caméra : seulement l'axe X
		// Conversion de l'angle X pour gérer les valeurs > 180°
		float xAngle = eulerAngles.x;
		if (xAngle > 180f) xAngle -= 360f;

		// Optionnel : Limiter l'angle vertical
		xAngle = Mathf.Clamp(xAngle, -80f, 80f);

		transform.localRotation = Quaternion.Euler(xAngle, 0, 0);
	}
}
