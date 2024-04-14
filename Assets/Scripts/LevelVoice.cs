using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelVoice : MonoBehaviour
{
	private AudioSource source;
	[SerializeField] private AudioClip clip;

	private void Start()
	{
		source = GetComponent<AudioSource>();
		source.PlayOneShot(clip);
	}
}
