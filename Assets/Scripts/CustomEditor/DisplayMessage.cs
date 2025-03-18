using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayMessage : MonoBehaviour
{
	public static DisplayMessage Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	private string customMessage;
	[SerializeField] private TMP_Text textMessage;

	public void NormalMessage(string message)
	{
		customMessage = message;
		StopAllCoroutines();
		StartCoroutine(Display());
	}

	public void WarningMessage(string message)
	{
		customMessage = $"<color=#FFD700>WARNING: {message}</color>";
		StopAllCoroutines();
		StartCoroutine(Display());
	}

	public void ErrorMessage(string message)
	{
		customMessage = $"<color=red>ERROR: {message}</color>";
		StopAllCoroutines();
		StartCoroutine(Display());
	}

	private IEnumerator Display()
	{
		textMessage.text = customMessage;
		textMessage.gameObject.SetActive(true);
		textMessage.color = new Color(1, 1, 1, 0);

		float a = 0;

		while(a < 1)
		{
			a += .01f;
			textMessage.color = new Color(1, 1, 1, a);
			yield return null;
		}

		a = 1;
		textMessage.color = new Color(1, 1, 1, a);

		yield return new WaitForSecondsRealtime(4);

		while (a > 0)
		{
			a -= .01f;
			textMessage.color = new Color(1, 1, 1, a);
			yield return null;
		}

		textMessage.color = new Color(1, 1, 1, 0);
		textMessage.gameObject.SetActive(false);
	}
}
