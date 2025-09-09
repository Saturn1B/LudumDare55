using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipsMenu : MonoBehaviour
{
	[SerializeField] private GameObject tooltipsPanel;
	[SerializeField] private TMP_Text subtitleText, mainText;

	public void CloseTooltip()
	{
		tooltipsPanel.SetActive(false);
	}

	public void OpenTooltip(string subtitle, string main)
	{
		tooltipsPanel.SetActive(true);
		subtitleText.text = subtitle;
		mainText.text = main;
	}
}