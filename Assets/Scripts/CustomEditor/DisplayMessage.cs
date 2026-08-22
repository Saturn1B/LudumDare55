using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
	[SerializeField] private Image textPanel;

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
		textPanel.gameObject.SetActive(true);
		textPanel.color = new Color(0, 0, 0, 0);
		textMessage.color = new Color(1, 1, 1, 0);

		float a = 0;

		while(a < 1)
		{
			a += .01f;
			textPanel.color = new Color(0, 0, 0, a / 10);
			textMessage.color = new Color(1, 1, 1, a);
			yield return null;
		}

		a = 1;
		textPanel.color = new Color(0, 0, 0, a / 10);
		textMessage.color = new Color(1, 1, 1, a);

		yield return new WaitForSecondsRealtime(4);

		while (a > 0)
		{
			a -= .01f;
			textPanel.color = new Color(0, 0, 0, a / 10);
			textMessage.color = new Color(1, 1, 1, a);
			yield return null;
		}

		textPanel.color = new Color(0, 0, 0, 0);
		textMessage.color = new Color(1, 1, 1, 0);
		textPanel.gameObject.SetActive(false);
	}
}
