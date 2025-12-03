using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TooltipsMenu : MonoBehaviour
{
	[SerializeField] private GameObject tooltipsPanel;
	[SerializeField] private TMP_Text subtitleTooltip, mainTooltip;

	[SerializeField] private GameObject guidePanel;
	[SerializeField] private TMP_Text subtitleGuide, mainGuide;
	[SerializeField] private Image displayImage;

	[SerializeField] private GameObject closeButton, nextButton, previousButton;

	[SerializeField] private GuideSlide[] guideSlides;
	private int currentSlide = 0;

	private void Start()
	{
		if(PlayerPrefs.GetInt("guideDone") != 1)
			OpenStartGuide();
	}

	public void CloseTooltip()
	{
		tooltipsPanel.SetActive(false);
		ShortcutManager.pauseDisponible = true;
	}

	public void OpenTooltip(string subtitle, string main)
	{
		tooltipsPanel.SetActive(true);
		subtitleTooltip.text = subtitle;
		mainTooltip.text = main;
		ShortcutManager.pauseDisponible = false;
	}

	public void OpenStartGuide()
	{
		if(PlayerPrefs.HasKey("guideDone") && PlayerPrefs.GetInt("guideDone") == 1)
			closeButton.SetActive(true);

		if (EditorHUDManager.Instance.isPaused)
			EditorHUDManager.Instance.PauseGame();

		guidePanel.SetActive(true);
		SetGuideSlide(0);
		currentSlide = 0;
		ShortcutManager.pauseDisponible = false;
	}

	void SetGuideSlide(int slideId)
	{
		if(slideId == 0)
		{
			previousButton.SetActive(false);
			nextButton.SetActive(true);
		}
		else if(slideId == guideSlides.Length - 1)
		{
			previousButton.SetActive(true);
			nextButton.SetActive(false);
		}

		if(slideId < guideSlides.Length - 1)
		{
			ToggleCloseButton(false);
			if (slideId > 0)
			{
				previousButton.SetActive(true);
				nextButton.SetActive(true);
			}
		}
		else
		{
			ToggleCloseButton(true);
		}

		subtitleGuide.text = guideSlides[slideId].subtitle;
		mainGuide.text = guideSlides[slideId].mainText;
		displayImage.sprite = guideSlides[slideId].displayImage;

		currentSlide = slideId;
	}

	public void MoveSlide(int amount)
	{
		currentSlide += amount;
		SetGuideSlide(currentSlide);
	}

	public void CloseGuide()
	{
		if (!PlayerPrefs.HasKey("guideDone"))
			PlayerPrefs.SetInt("guideDone", 1);

		guidePanel.SetActive(false);
		ShortcutManager.pauseDisponible = true;
	}

	public void ToggleCloseButton(bool value)
	{
		if (PlayerPrefs.HasKey("guideDone") && PlayerPrefs.GetInt("guideDone") == 1)
			return;

		closeButton.SetActive(value);
	}
}

[System.Serializable]
public class GuideSlide
{
	public string subtitle;
	[TextArea(5, 5)] public string mainText;
	public Sprite displayImage;
}