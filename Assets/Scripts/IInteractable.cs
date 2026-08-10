
using UnityEngine;

public interface IInteractable
{
	public bool CanInteract();
	public void Interact(Transform user);
	public void EndInteraction();
}
