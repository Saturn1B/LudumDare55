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
	[SerializeField] private GameObject topSection, bottomSection, createSection, onlineBottomSection;
	[SerializeField] private TMP_Text titleText;
	[SerializeField] private Image levelImage;
	[SerializeField] private RectTransform levelImageMask;
	[SerializeField] private UnityEngine.UI.Button createLevelButton, deleteLevelButton, likeButton;
	[SerializeField] private Image likeImage;
	[SerializeField] private Sprite emptyHeart, fillHeart;
	[SerializeField] private TMP_Text likeCount;

	bool isLevelLiked;

	private SceneData currentSceneData;

	public void SetCard(LevelCardType levelCardType ,SceneData sceneData = null, Action<SceneData, GameObject> deleteCallback = null, Action createCallback = null)
	{
		switch (levelCardType)
		{
			case LevelCardType.CREATE:
				topSection.SetActive(false);
				bottomSection.SetActive(false);
				onlineBottomSection.SetActive(false);
				createSection.SetActive(true);
				createLevelButton.onClick.AddListener(() => { createCallback?.Invoke(); });
				break;
			case LevelCardType.EDIT:
				topSection.SetActive(true);
				bottomSection.SetActive(true);
				onlineBottomSection.SetActive(false);
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
	public async void SetOnlineCard(LevelData levelData)
	{
		topSection.SetActive(true);
		bottomSection.SetActive(false);
		onlineBottomSection.SetActive(true);
		createSection.SetActive(false);
		titleText.text = levelData.sceneData.levelName;

		if (levelData != null)
			likeButton.onClick.AddListener(() => { ToggleLikeLevelButton(levelData); });

			currentSceneData = levelData.sceneData;
		likeCount.text = levelData.likesCount.ToString();

		isLevelLiked = await FirestoreManager.Instance.HasUserLiked(levelData.uploadId);
		if(isLevelLiked)
			likeImage.sprite = fillHeart;
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
		LevelDataTransfer.isEditing = true;
		SceneManager.LoadScene("EditorScene", LoadSceneMode.Single);
	}

	public void PlayLevelButton()
	{
		LevelDataTransfer.SceneDataToLoad = currentSceneData;
		LevelDataTransfer.levelName = currentSceneData.levelName;
		LevelDataTransfer.isEditing = false;
		SceneManager.LoadScene("PlayScene", LoadSceneMode.Single);
	}

	public async void ToggleLikeLevelButton(LevelData levelData)
	{
		await FirestoreManager.Instance.ToggleLikeLevel(levelData.uploadId);

		isLevelLiked = await FirestoreManager.Instance.HasUserLiked(levelData.uploadId);

		int likes = await FirestoreManager.Instance.GetLikeCount(levelData.uploadId);
		likeCount.text = likes.ToString();

		if (!isLevelLiked)
		{
			likeImage.sprite = emptyHeart;
		}
		else
		{
			likeImage.sprite = fillHeart;
		}
	}
}
