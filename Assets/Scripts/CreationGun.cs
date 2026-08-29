using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Materials
{
	EMPTY = 0,
	COMPOSITE = 1,
	ALUMINUM = 2,
	GLASS = 3
}

public class CreationGun : MonoBehaviour
{
	[SerializeField] private GameObject prefabObject;
	[SerializeField] private GameObject shoulder, barrelEnd;
	[SerializeField] private Camera playerCamera;

	private Materials materials;

	private GameObject[] currentObject;
	private int[] currentIndex;

	[SerializeField] private GameObject[] compositeObjects;
	[SerializeField] private GameObject[] aluminumObjects;
	[SerializeField] private GameObject[] glassObjects;

	[SerializeField] private Material[] ringColor;
	[SerializeField] private GameObject[] rings;

	[SerializeField] private Image objectIcon;
	[SerializeField] private Sprite emptyIcon;

	[SerializeField] private LayerMask layer;

	[Header("Object pick up")]
	[SerializeField] private LayerMask summonObjectLayer;
	[SerializeField] private float spring = 500f;
	[SerializeField] private float damper = 50f;
	[SerializeField] private float maxForce = 250f;
	[SerializeField] private float baseRotationSpeed;
	private Rigidbody holder;
	private Transform currentpickedUp;
	private ConfigurableJoint currentJoint;
	private float holdDistance;
	private float currentYaw;
	private float yawVelocity;

	private CharacterMovement characterMovement;

	private void Start()
	{
		holder = GetComponentInChildren<Rigidbody>();
		characterMovement = GetComponentInParent<CharacterMovement>();

		foreach (var ring in rings)
		{
			ring.GetComponent<MeshRenderer>().material = ringColor[(int)materials];
		}

		currentIndex = new int[3];
	}

	private void Update()
	{
		if (currentpickedUp != null)
		{
			Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
			holder.MovePosition(targetPos);

			//Rotate picked object with mouse wheel
			float scroll = Input.mouseScrollDelta.y;

			float currentMass = Mathf.Sqrt(currentpickedUp.GetComponent<Rigidbody>().mass);

			if (Mathf.Abs(scroll) > .01f)
			{
				yawVelocity += (scroll * 20) / currentMass;
			}

			float maxSpeedForMass = 2000 / currentMass;
			yawVelocity = Mathf.Clamp(yawVelocity, -maxSpeedForMass, maxSpeedForMass);

			yawVelocity = Mathf.Lerp(yawVelocity, 0f, 3 * Time.deltaTime);

			currentYaw += yawVelocity * Time.deltaTime;

			holder.transform.rotation = Quaternion.AngleAxis(currentYaw, Vector3.up);
		}

		if (shoulder != null)
			shoulder.transform.localEulerAngles = new Vector3(playerCamera.transform.localEulerAngles.x, 0, 0);

		RaycastHit hit;
		//Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 100, Color.red, 1);
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 100, ~layer))
		{
			//Right clic
			if (currentpickedUp == null && Input.GetKeyDown(KeyCode.Mouse1) && materials != Materials.EMPTY)
			{
				Vector3 objectSize = prefabObject.GetComponentInChildren<Renderer>().bounds.size;
				Vector3 offset = objectSize * 0.5f;

				Vector3 summonPoint = hit.point + hit.normal * offset.magnitude;
				Quaternion summonRotation = Quaternion.Euler(playerCamera.transform.parent.parent.localEulerAngles);

				if (!IsSpaceFree(summonPoint, summonRotation, offset)) return;

				GameObject go = Instantiate(currentObject[currentIndex[(int)materials - 1]], summonPoint, Quaternion.identity);
				go.transform.localEulerAngles = playerCamera.transform.parent.parent.localEulerAngles;
				materials = Materials.EMPTY;
				foreach (var ring in rings)
				{
					ring.GetComponent<MeshRenderer>().material = ringColor[(int)materials];
				}
				objectIcon.sprite = emptyIcon;
			}

			//Middle clic
			if (currentpickedUp == null && Input.GetKeyDown(KeyCode.Mouse2) && hit.transform.CompareTag("Object"))
			{
				if (materials == Materials.EMPTY)
				{
					SwitchMaterials(hit.transform.GetComponent<Object>().objectMaterial);
					Destroy(hit.transform.gameObject);
				}
			}
		}

		//Left clic
		if (Input.GetKeyDown(KeyCode.Mouse0))
		{
			Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

			if (Physics.Raycast(ray, out RaycastHit hit2, 5, summonObjectLayer))
			{
				Debug.Log("pickObject");
				PickUp(hit2.transform.gameObject);
				return;
			}

			if (materials == Materials.EMPTY) return;

			Vector3 objectSize = prefabObject.GetComponentInChildren<Renderer>().bounds.size;
			Vector3 offset = objectSize * 0.5f;

			Vector3 summonPoint = barrelEnd.transform.position + barrelEnd.transform.forward * offset.z;
			Quaternion summonRotation = Quaternion.Euler(playerCamera.transform.parent.parent.localEulerAngles);

			if (!IsSpaceFree(summonPoint, summonRotation, offset)) return;

			GameObject go = Instantiate(currentObject[currentIndex[(int)materials - 1]], summonPoint, Quaternion.identity);
			go.transform.localEulerAngles = playerCamera.transform.parent.parent.localEulerAngles;
			go.GetComponent<Rigidbody>().AddForce(playerCamera.transform.forward * 20, ForceMode.Impulse);

			materials = Materials.EMPTY;
			foreach (var ring in rings)
			{
				ring.GetComponent<MeshRenderer>().material = ringColor[(int)materials];
			}
			objectIcon.sprite = emptyIcon;
		}

		//Release left clic
		if (currentpickedUp != null && Input.GetKeyUp(KeyCode.Mouse0))
		{
			Release();
		}

		if (materials != Materials.EMPTY)
		{
			//MouseScroll
			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				currentIndex[(int)materials - 1] = (currentIndex[(int)materials - 1] + 1) >= currentObject.Length ? 0 : currentIndex[(int)materials - 1] + 1;
			}
			else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				currentIndex[(int)materials - 1] = (currentIndex[(int)materials - 1] - 1) < 0 ? currentObject.Length - 1 : currentIndex[(int)materials - 1] - 1;
			}

			objectIcon.sprite = currentObject[currentIndex[(int)materials - 1]].GetComponent<Object>().objectImage;
		}
	}

	private bool IsSpaceFree(Vector3 position, Quaternion rotation, Vector3 halfExtents)
	{
		Collider[] overlaps = Physics.OverlapBox(position, halfExtents * 0.95f, rotation, ~layer, QueryTriggerInteraction.Ignore);
		Debug.Log(overlaps.Length);
		return overlaps.Length == 0;
	}

	public bool SwitchMaterials(Materials newMat)
	{
		if (materials != Materials.EMPTY) return false;

		materials = newMat;

		switch (materials)
		{
			case Materials.COMPOSITE:
				currentObject = compositeObjects;
				break;
			case Materials.ALUMINUM:
				currentObject = aluminumObjects;
				break;
			case Materials.GLASS:
				currentObject = glassObjects;
				break;
			default:
				currentObject = compositeObjects;
				break;
		}

		objectIcon.sprite = currentObject[currentIndex[(int)materials - 1]].GetComponent<Object>().objectImage;

		foreach (var ring in rings)
		{
			ring.GetComponent<MeshRenderer>().material = ringColor[(int)materials];
		}

		return true;
	}

	private void PickUp(GameObject toPickUp)
	{
		holdDistance = Vector3.Distance(playerCamera.transform.position, toPickUp.transform.position);

		Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
		holder.transform.position = targetPos;

		currentJoint = toPickUp.AddComponent<ConfigurableJoint>();

		currentJoint.connectedBody = holder;

		currentJoint.autoConfigureConnectedAnchor = false;
		currentJoint.anchor = Vector3.zero;
		currentJoint.connectedAnchor = Vector3.zero;

		JointDrive drive = new JointDrive { positionSpring = spring, positionDamper = damper, maximumForce = maxForce };
		currentJoint.xDrive = drive;
		currentJoint.yDrive = drive;
		currentJoint.zDrive = drive;

		currentJoint.angularXDrive = drive;
		currentJoint.slerpDrive = drive;
		currentJoint.rotationDriveMode = RotationDriveMode.Slerp;

		currentpickedUp = toPickUp.transform;
		characterMovement.SetHeldObject(toPickUp.transform);
	}

	private void Release()
	{
		Destroy(currentJoint);
		currentpickedUp = null;
		holder.transform.localPosition = Vector3.zero;
		currentYaw = 0;
		characterMovement.SetHeldObject(null);
	}
}
