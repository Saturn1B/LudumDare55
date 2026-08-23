using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(UnityEngine.CharacterController))]
public class CharacterMovement : MonoBehaviour
{
	[Header("Camera Control")]
	[SerializeField] private float minPitch;
	[SerializeField] private float maxPitch;
	[SerializeField] private float lookSensitivity;

	[Header("Player Movement")]
	[SerializeField] private float moveSpeed;
	[SerializeField] private float sprintSpeed;
	[SerializeField] private float crouchSpeed;
	[SerializeField] private float jumpHeight;
	[SerializeField] private float airControl = 2f;
	[SerializeField] private float maxAirSpeed = 6f;
	[SerializeField] private float airDrag = 0.3f;
	[SerializeField] private float beltForce = 16f;

	[Header("Crouch")]
	[SerializeField] private bool canCrouch;
	[SerializeField] private float standingHeight;
	[SerializeField] private float crouchingHeight;

	private UnityEngine.CharacterController characterController;
	private Camera playerCamera;

	private float yaw;
	private float pitch;

	private Vector3 velocity = Vector3.zero;
	private float gravity = 9.81f;
	private bool isCrouching;
	private float crouchTransitionSpeed = .1f;

	private bool canMove = true;

	private Transform activePlatform;
	private Vector3 lastPlatformPosition;
	private bool isTouchingPlatformThisFrame;

	private bool isOnBelt;
	private Vector3 beltDirection = Vector3.zero;
	private bool isTouchingBeltThisFrame;

	private void Start()
	{
		characterController = GetComponent<UnityEngine.CharacterController>();
		playerCamera = GetComponentInChildren<Camera>();

		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}

	private void Update()
	{
		HandleMouseLook();

		if (!canMove)
		{
			velocity.y -= gravity * Time.deltaTime;
			characterController.Move(velocity * Time.deltaTime);

			return;
		}

		HandlePlatformMovement();
		HandleBeltState();
		HandleMovement();
		if (canCrouch)
			HandleCrouch();
	}

	private void HandlePlatformMovement()
	{
		if (activePlatform != null)
		{
			if (!isTouchingPlatformThisFrame)
			{
				activePlatform = null;
				return;
			}

			Vector3 platformDelta = activePlatform.position - lastPlatformPosition;
			if (platformDelta != Vector3.zero)
			{
				characterController.Move(platformDelta);
			}
			lastPlatformPosition = activePlatform.position;

			isTouchingPlatformThisFrame = false;
		}
	}

	private void HandleBeltState()
	{
		isOnBelt = isTouchingBeltThisFrame;
		isTouchingBeltThisFrame = false;
	}

	private void HandleMovement()
	{
		bool grounded = characterController.isGrounded || activePlatform != null;

		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		Vector3 inputDirection = transform.rotation * new Vector3(horizontal, 0.0f, vertical);

		HandleJump();

		if (grounded)
		{
			float currentSpeed = isCrouching ? crouchSpeed : Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : moveSpeed;
			Vector3 moveDirection = inputDirection * currentSpeed;

			velocity.x = moveDirection.x;
			velocity.z = moveDirection.z;
		}
		else
		{
			Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
			Vector3 addedVelocity = inputDirection * airControl * Time.deltaTime;

			Vector3 newHorizontalVelocity = horizontalVelocity + addedVelocity;

			if (newHorizontalVelocity.magnitude > horizontalVelocity.magnitude &&
				newHorizontalVelocity.magnitude > maxAirSpeed)
			{
				newHorizontalVelocity = newHorizontalVelocity.normalized * Mathf.Max(maxAirSpeed, horizontalVelocity.magnitude);
			}

			newHorizontalVelocity *= Mathf.Exp(-airDrag * Time.deltaTime);

			velocity.x = newHorizontalVelocity.x;
			velocity.z = newHorizontalVelocity.z;
		}

		if (isOnBelt)
		{
			velocity.x += beltDirection.x * beltForce;
			velocity.z += beltDirection.z * beltForce;
		}

		characterController.Move(velocity * Time.deltaTime);
	}

	private void HandleMouseLook()
	{
		yaw += Input.GetAxisRaw("Mouse X") * lookSensitivity;
		pitch -= Input.GetAxisRaw("Mouse Y") * lookSensitivity;

		pitch = ClampAngle(pitch, minPitch, maxPitch);

		transform.eulerAngles = new Vector3(0.0f, yaw, 0.0f);
		playerCamera.transform.localEulerAngles = new Vector3(pitch, 0.0f, 0.0f);
	}

	public void SetYPlayerAngle(Vector3 newAngle)
	{
		yaw = newAngle.y;
	}

	private void HandleJump()
	{
		bool grounded = characterController.isGrounded || activePlatform != null;

		if (grounded)
		{
			if (velocity.y < 0)
			{
				velocity.y = -2f;
			}

			if (Input.GetKeyDown(KeyCode.Space))
			{
				velocity.y = jumpHeight;
				activePlatform = null;
			}
		}
		else
		{
			velocity.y -= gravity * Time.deltaTime;
		}
	}

	private void HandleCrouch()
	{
		if (Input.GetKeyDown(KeyCode.LeftControl))
		{
			StartCoroutine(CrouchStand());
		}
	}

	private IEnumerator CrouchStand()
	{
		float targetHeight = isCrouching ? standingHeight : crouchingHeight;
		float initialHeight = characterController.height;

		float timeElapsed = 0f;

		while (timeElapsed < crouchTransitionSpeed)
		{
			characterController.height = Mathf.Lerp(initialHeight, targetHeight, timeElapsed / crouchTransitionSpeed);
			timeElapsed += Time.deltaTime;
			yield return null;
		}

		characterController.height = targetHeight;
		isCrouching = !isCrouching;
	}

	private float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360)
			angle += 360;
		if (angle > 360)
			angle -= 360;

		return Mathf.Clamp(angle, min, max);
	}

	float pushPower = .5f;

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		if (hit.moveDirection.y < -0.9f && hit.normal.y > 0.5f)
		{
			if (hit.collider.CompareTag("MovingPlatform"))
			{
				isTouchingPlatformThisFrame = true;

				if (activePlatform != hit.collider.transform)
				{
					activePlatform = hit.collider.transform;
					lastPlatformPosition = activePlatform.position;
				}
			}
			if (hit.transform.TryGetComponent(out JumpPad jumpPad))
			{
				velocity.y = 10;
			}
			if (hit.transform.TryGetComponent(out Belt belt))
			{
				isTouchingBeltThisFrame = true;
				beltDirection = hit.transform.forward;
			}
		}

		Rigidbody body = hit.collider.attachedRigidbody;

		if (body == null || body.isKinematic) return;

		if (hit.moveDirection.y < -0.3f) return;

		Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

		body.AddForceAtPosition(pushDir * pushPower, hit.point, ForceMode.Impulse);
	}
}