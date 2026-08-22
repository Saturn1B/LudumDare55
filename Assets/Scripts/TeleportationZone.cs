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
			{
				if(LevelDataTransfer.isEditing == false)
					SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
				else
					SceneManager.LoadScene("EditorScene", LoadSceneMode.Single);
			}
			//else
				//TO DO ending panel
		}
	}
}
