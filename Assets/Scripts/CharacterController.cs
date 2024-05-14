using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float speed;
    [SerializeField] private float maxStepHeight;

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
        RaycastHit hit;
        bool isGrounded = Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hit, checkDistance, groundLayer);

        HandleMouseLook();

        if (isGrounded)
        {
            Jump();
        }
    }

	private void FixedUpdate()
	{
        RaycastHit hit;
        bool isGrounded = Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hit, checkDistance, groundLayer);

        float currSpeed = isGrounded ? speed : speed / 16;

        Move(currSpeed);
    }

    private void Move(float currSpeed)
	{
        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");

        Vector3 forwardDirection = transform.forward;

        Vector3 movementDirection = (forwardDirection * zInput + transform.right * xInput).normalized;

        RaycastHit hit;
        Vector3 bottom = transform.position - new Vector3(0, GetComponent<CapsuleCollider>().bounds.extents.y, 0);
        bool onStep = Physics.Raycast(bottom, movementDirection, out hit, .5f);

        if (onStep && hit.transform.GetComponent<Collider>() && hit.transform.GetComponent<Collider>().bounds.extents.y * 2 <= maxStepHeight)
        {
            Vector3 stepOffset = new Vector3(0, hit.transform.GetComponent<Collider>().bounds.extents.y * 2, 0);
            transform.position += stepOffset;
        }

        rb.AddForce(movementDirection * currSpeed * Time.fixedDeltaTime, ForceMode.VelocityChange);
	}

    private void Jump()
	{
		if (Input.GetKeyDown(KeyCode.Space))
		{
            rb.AddForce(Vector3.up * jumpForce);
		}
	}

    private float smoothedYaw;
    private float smoothedPitch;

    private void HandleMouseLook()
    {
        // Camera Look
        yaw += Input.GetAxis("Mouse X") * lookSensitivity;
        pitch -= Input.GetAxis("Mouse Y") * lookSensitivity;

        pitch = ClampAngle(pitch, minPitch, maxPitch);

        smoothedYaw = Mathf.Lerp(smoothedYaw, yaw, 50 * Time.deltaTime);
        smoothedPitch = Mathf.Lerp(smoothedPitch, pitch, 50 * Time.deltaTime);

        transform.eulerAngles = new Vector3(0.0f, smoothedYaw, 0.0f);
        playerCamera.transform.localEulerAngles = new Vector3(smoothedPitch, 0.0f, 0.0f);
    }

    private float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360)
            angle += 360;
        if (angle > 360)
            angle -= 360;

        return Mathf.Clamp(angle, min, max);
    }

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Kill") || other.CompareTag("Laser"))
		{
            SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
		}
	}
}
