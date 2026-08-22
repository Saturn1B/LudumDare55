using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipsDialog : MonoBehaviour
{
	[SerializeField] private TMP_Text tooltipsText;
	[SerializeField] private Image dialogBox;

	private void Start()
	{
		dialogBox.color = new Color(dialogBox.color.r, dialogBox.color.g, dialogBox.color.b, 0);
		tooltipsText.color = new Color(tooltipsText.color.r, tooltipsText.color.g, tooltipsText.color.b, 0);
	}

	public void ShowTooltips(string text, Vector3 position, float halfSize)
	{
		tooltipsText.text = text;
		if(position.x < 160)
			position = new Vector3(160, position.y, position.z);
		if(position.x > Screen.width - 160)
			position = new Vector3(Screen.width - 160, position.y, position.z);
		if(position.y <= Screen.height / 2)
			transform.position = position + Vector3.up * (halfSize + 25);
		else
			transform.position = position - Vector3.up * (halfSize + 25);

		Debug.Log(transform.position);

		StopAllCoroutines();
		StartCoroutine(Fading(true, .03f, 1));
	}

    public void HideTooltips()
	{
		StopAllCoroutines();
		StartCoroutine(Fading(false, -.03f, 0));
	}

	private IEnumerator Fading(bool activeState, float alphaStep, float alpha)
	{
		if (activeState) dialogBox.gameObject.SetActive(activeState);

		while (dialogBox.color.a <= 1 && dialogBox.color.a >= 0)
		{
			dialogBox.color += new Color(0, 0, 0, alphaStep);
			tooltipsText.color += new Color(0, 0, 0, alphaStep);
			yield return null;
		}

		dialogBox.color = new Color(dialogBox.color.r, dialogBox.color.g, dialogBox.color.b, alpha);
		tooltipsText.color = new Color(tooltipsText.color.r, tooltipsText.color.g, tooltipsText.color.b, alpha);

		if (!activeState) dialogBox.gameObject.SetActive(activeState);
	}
}
