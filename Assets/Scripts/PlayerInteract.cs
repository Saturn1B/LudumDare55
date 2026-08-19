using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerInteract : MonoBehaviour
{
	[SerializeField] private Camera viewCamera;
	[SerializeField] private float playerReach, feetReach;
	[SerializeField] private Transform feet;

	private IInteractable interactable;
	private IPlayerDetector playerDetector;

	void Update()
	{
		Ray ray2 = new Ray(feet.position, feet.up * -1);

		if (playerDetector == null && Physics.Raycast(ray2, out RaycastHit hit2, feetReach))
		{
			if (hit2.transform.TryGetComponent(out playerDetector))
			{
				playerDetector.StartDetection(transform);
			}
		}
		if (playerDetector != null && !Physics.Raycast(ray2, out RaycastHit hit3, feetReach))
		{
			playerDetector.EndDetection();
			playerDetector = null;
		}


		if (viewCamera == null) return;

		Ray ray = new Ray(viewCamera.transform.position, viewCamera.transform.forward);

		if (!Physics.Raycast(ray, out RaycastHit hit, playerReach))
		{
			HUDManager.Instance.HideIndication();
			return;
		}

		if (!hit.transform.TryGetComponent(out interactable))
		{
			HUDManager.Instance.HideIndication();
			return;
		}

		if (!interactable.CanInteract())
		{
			HUDManager.Instance.HideIndication();
			return;
		}

		HUDManager.Instance.DiplayIndication();

		if (Input.GetKey(KeyCode.E))
			interactable.Interact(this.transform);
		else if (Input.GetKeyUp(KeyCode.E))
		{
			if (interactable != null)
				interactable.EndInteraction();
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Kill") || other.CompareTag("Laser"))
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
		}
	}
}
