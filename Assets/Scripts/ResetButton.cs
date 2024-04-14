using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetButton : MonoBehaviour
{
	private bool hasPlayer;
	[SerializeField] private Animator animator;

	private void Update()
	{
		if (hasPlayer && Input.GetKeyDown(KeyCode.E))
		{
			StartCoroutine(ButtonPress());
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			hasPlayer = true;
			HUDManager.Instance.DiplayIndication();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			hasPlayer = false;
			HUDManager.Instance.HideIndication();
		}
	}

	private IEnumerator ButtonPress()
	{
		animator.Play("Base Layer.ButtonPress", 0, 0);
		yield return new WaitForSeconds(0.6f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
	}
}
