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
		source = GetComponent<AudioSource>();

		if (!PlayerPrefs.HasKey(SceneManager.GetActiveScene().name) || PlayerPrefs.GetInt(SceneManager.GetActiveScene().name) == 0)
		{
			StartCoroutine(PlayAudio());
		}
	}

	private IEnumerator PlayAudio()
	{
		source.PlayOneShot(clip);
		yield return new WaitForSeconds(clip.length);
		PlayerPrefs.SetInt(SceneManager.GetActiveScene().name, 1);
	}
}
