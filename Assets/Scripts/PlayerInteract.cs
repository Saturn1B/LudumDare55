using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
	[SerializeField] private float playerReach;

    private IInteractable interactable;

    void Update()
    {
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
}
