using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetButton : MonoBehaviour, IInteractable
{
	[SerializeField] private Animator animator;

	private IEnumerator ButtonPress()
	{
		animator.Play("Base Layer.ButtonPress", 0, 0);
		yield return new WaitForSeconds(0.6f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
	}

	public void Interact(Transform user)
	{
		StartCoroutine(ButtonPress());
	}

	public void EndInteraction()
	{
	}

	public bool CanInteract() => true;
}
