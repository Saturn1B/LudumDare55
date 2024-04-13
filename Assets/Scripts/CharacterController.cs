using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed;

    [Space]

    [Header("Jump")]
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float checkDistance;

    [Space]

    [Header("Camera Control")]
    [SerializeField] private float minPitch;
    [SerializeField] private float maxPitch;
    [SerializeField] private float lookSensitivity;
    private Camera playerCamera;
    private float yaw;
    private float pitch;

    private void Start()
	{
        rb = GetComponent<Rigidbody>();
        playerCamera = GetComponentInChildren<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

	void Update()
    {
        HandleMouseLook();
        Move();

        RaycastHit hit;
        bool isGrounded = Physics.Raycast(transform.position, Vector3.down, out hit, checkDistance, groundLayer);

        if (isGrounded)
        {
            Jump();
        }
    }

	private void Move()
	{
        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");

        Vector3 forwardDirection = transform.forward;
        Vector3 movementDirection = (forwardDirection * zInput + transform.right * xInput).normalized;

        rb.AddForce(movementDirection * speed * Time.deltaTime, ForceMode.VelocityChange);

        //rb.velocity = new Vector3(movementDirection.x * speed, rb.velocity.y, movementDirection.z * speed);
	}

    private void Jump()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
            rb.AddForce(Vector3.up * jumpForce);
		}
	}

    private void HandleMouseLook()
    {
        // Camera Look
        yaw += Input.GetAxisRaw("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxisRaw("Mouse Y") * lookSensitivity;

        pitch = ClampAngle(pitch, minPitch, maxPitch);

        transform.eulerAngles = new Vector3(0.0f, yaw, 0.0f);
        playerCamera.transform.localEulerAngles = new Vector3(pitch, 0.0f, 0.0f);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }
}
