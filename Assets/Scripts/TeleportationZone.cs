using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeleportationZone : MonoBehaviour
{
	[SerializeField] private string nextScene;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			if (!string.IsNullOrEmpty(nextScene))
				SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
			//else
				//TO DO ending panel
		}
	}
}
