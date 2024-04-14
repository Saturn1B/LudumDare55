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
	private int currentIndex;

	[SerializeField] private GameObject[] compositeObjects;
	[SerializeField] private GameObject[] aluminumObjects;
	[SerializeField] private GameObject[] glassObjects;

	[SerializeField] private Material[] ringColor;
	[SerializeField] private GameObject[] rings;

	[SerializeField] private Image objectIcon;
	[SerializeField] private Sprite emptyIcon;

	public int test;

	private void Start()
	{
		foreach (var ring in rings)
		{
			ring.GetComponent<MeshRenderer>().material = ringColor[(int)materials];
		}
	}

	private void Update()
	{
		shoulder.transform.localEulerAngles = new Vector3(playerCamera.transform.localEulerAngles.x, 0, 0);

		RaycastHit hit;
		Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * 100, Color.red, 1);
		if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 100))
		{
			//Right clic
			if (Input.GetKeyDown(KeyCode.Mouse1) && materials != Materials.EMPTY)
			{
				Vector3 objectSize = prefabObject.GetComponentInChildren<Renderer>().bounds.size;

				Vector3 offset = objectSize * 0.5f;
				var offsety = objectSize.y * 0.5f;

				Vector3 summonPoint = hit.point + hit.normal * offset.magnitude;
				GameObject go = Instantiate(currentObject[currentIndex], summonPoint, Quaternion.identity);
				go.transform.localEulerAngles = playerCamera.transform.parent.localEulerAngles;
				materials = Materials.EMPTY;
				foreach (var ring in rings)
				{
					ring.GetComponent<MeshRenderer>().material = ringColor[(int)materials];
				}
				objectIcon.sprite = emptyIcon;
			}

			//Middle clic
			if (Input.GetKeyDown(KeyCode.Mouse2) && hit.transform.CompareTag("Object"))
			{
				if (materials == Materials.EMPTY)
				{
					SwitchMaterials(hit.transform.GetComponent<Object>().objectMaterial);
					Destroy(hit.transform.gameObject);
				}
			}
		}

		//Left clic
		if (Input.GetKeyDown(KeyCode.Mouse0) && materials != Materials.EMPTY)
		{
			Vector3 objectSize = prefabObject.GetComponentInChildren<Renderer>().bounds.size;

			Vector3 offset = objectSize * 0.5f;

			Vector3 summonPoint = barrelEnd.transform.position + barrelEnd.transform.forward * offset.z;

			GameObject go = Instantiate(currentObject[currentIndex], summonPoint, Quaternion.identity);
			go.transform.localEulerAngles = playerCamera.transform.parent.localEulerAngles;
			go.GetComponent<Rigidbody>().AddForce(playerCamera.transform.forward * 20, ForceMode.Impulse);

			materials = Materials.EMPTY;
			foreach (var ring in rings)
			{
				ring.GetComponent<MeshRenderer>().material = ringColor[(int)materials];
			}
			objectIcon.sprite = emptyIcon;
		}

		if (materials != Materials.EMPTY)
		{
			//MouseScroll
			if (Input.GetAxis("Mouse ScrollWheel") > 0f)
			{
				currentIndex = (currentIndex + 1) >= currentObject.Length ? 0 : currentIndex + 1;
			}
			else if (Input.GetAxis("Mouse ScrollWheel") < 0f)
			{
				currentIndex = (currentIndex - 1) < 0 ? currentObject.Length - 1 : currentIndex - 1;
			}

			objectIcon.sprite = currentObject[currentIndex].GetComponent<Object>().objectImage;
		}
	}

	public void SwitchMaterials(Materials newMat)
	{
		if (materials != Materials.EMPTY) return;

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

		currentIndex = 0;
		objectIcon.sprite = currentObject[currentIndex].GetComponent<Object>().objectImage;

		foreach (var ring in rings)
		{
			ring.GetComponent<MeshRenderer>().material = ringColor[(int)materials];
		}
	}
}
