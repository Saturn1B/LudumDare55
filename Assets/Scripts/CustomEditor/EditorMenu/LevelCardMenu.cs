using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
using System;

public enum LevelCardType
{
	CREATE,
	EDIT
}

public class LevelCardMenu : MonoBehaviour
{
	[SerializeField] private GameObject topSection, bottomSection, createSection;
	[SerializeField] private TMP_Text titleText;
	[SerializeField] private Image levelImage;
	[SerializeField] private RectTransform levelImageMask;
	[SerializeField] private UnityEngine.UI.Button createLevelButton, deleteLevelButton;

	private SceneData currentSceneData;

	public void SetCard(LevelCardType levelCardType ,SceneData sceneData = null, Action<SceneData, GameObject> deleteCallback = null, Action createCallback = null)
	{
		switch (levelCardType)
		{
			case LevelCardType.CREATE:
				topSection.SetActive(false);
				bottomSection.SetActive(false);
				createSection.SetActive(true);
				createLevelButton.onClick.AddListener(() => { createCallback?.Invoke(); });
				break;
			case LevelCardType.EDIT:
				topSection.SetActive(true);
				bottomSection.SetActive(true);
				createSection.SetActive(false);
				titleText.text = sceneData.levelName;

				if(sceneData != null)
					deleteLevelButton.onClick.AddListener(() => { deleteCallback?.Invoke(sceneData, gameObject); });

				Sprite imageSprite = LoadPNG(SaveSystem.saveFilePath + sceneData.levelName + sceneData.levelId + ".png");

				SpriteFitter(imageSprite, levelImage, levelImageMask);

				//levelImage.sprite = LoadPNG(SaveSystem.saveFilePath + sceneData.levelName + ".png");
				levelImage.color = Color.white;
				break;
			default:
				topSection.SetActive(false);
				bottomSection.SetActive(false);
				createSection.SetActive(true);
				break;
		}

		currentSceneData = sceneData;
	}

	Sprite LoadPNG(string filePath)
	{
		Texture2D tex = null;
		byte[] fileData;

		if (System.IO.File.Exists(filePath))
		{
			fileData = System.IO.File.ReadAllBytes(filePath);
			tex = new Texture2D(2, 2);
			tex.LoadImage(fileData);
		}

		Rect rec = new Rect(0, 0, tex.width, tex.height);

		return Sprite.Create(tex, rec, Vector2.zero,1);
	}

	private void SpriteFitter(Sprite newSprite, Image image, RectTransform maskRect)
	{
		// Set the sprite to the Image component
		image.sprite = newSprite;

		// Get the dimensions of the sprite (its width and height)
		float spriteWidth = newSprite.rect.width;
		float spriteHeight = newSprite.rect.height;

		// Get the size of the mask (parent RectTransform)
		float maskWidth = maskRect.rect.width;
		float maskHeight = maskRect.rect.height;

		// Calculate the aspect ratio of the sprite and the mask
		float spriteAspect = spriteWidth / spriteHeight;
		float maskAspect = maskWidth / maskHeight;

		// Scale the image to fit the mask while maintaining the sprite's aspect ratio
		if (spriteAspect > maskAspect)
		{
			// Sprite is wider than the mask (scale width to fit mask width)
			float scale = maskWidth / spriteWidth;
			image.rectTransform.sizeDelta = new Vector2(maskWidth, spriteHeight * scale);
		}
		else
		{
			// Sprite is taller than the mask (scale height to fit mask height)
			float scale = maskHeight / spriteHeight;
			image.rectTransform.sizeDelta = new Vector2(spriteWidth * scale, maskHeight);
		}
	}

	public void EditLevelButton()
	{
		LevelDataTransfer.SceneDataToLoad = currentSceneData;
		LevelDataTransfer.levelName = currentSceneData.levelName;
		SceneManager.LoadScene("EditorScene", LoadSceneMode.Single);
	}

	public void PlayLevelButton()
	{
		LevelDataTransfer.SceneDataToLoad = currentSceneData;
		LevelDataTransfer.levelName = currentSceneData.levelName;
		SceneManager.LoadScene("PlayScene", LoadSceneMode.Single);
	}
}
