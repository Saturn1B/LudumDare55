using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelVoice : MonoBehaviour
{
	private AudioSource source;
	[SerializeField] private AudioClip clip;

	private void Start()
	{
		if (!PlayerPrefs.HasKey(SceneManager.GetActiveScene().name) || PlayerPrefs.GetInt(SceneManager.GetActiveScene().name) == 0)
		{
			source = GetComponent<AudioSource>();
			source.PlayOneShot(clip);
			PlayerPrefs.SetInt(SceneManager.GetActiveScene().name, 1);
		}

	}
}
