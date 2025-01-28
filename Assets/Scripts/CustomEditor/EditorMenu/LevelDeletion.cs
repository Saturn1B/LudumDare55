using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class LevelDeletion : MonoBehaviour
{
	[SerializeField] private TMP_Text toDeleteText;
	private SceneData toDeleteSceneData;
	private GameObject toDeleteCard;

	public void Setup(SceneData sceneData, GameObject levelCard)
	{
		toDeleteSceneData = sceneData;
		toDeleteCard = levelCard;
		toDeleteText.text = $"Are you sure you want to delete this level : <color=#FFD700>{toDeleteSceneData.levelName}</color>";
	}

	public void DeleteLevel()
	{
		File.Delete(SaveSystem.saveFilePath + toDeleteSceneData.levelName + toDeleteSceneData.levelId + ".json");
		File.Delete(SaveSystem.saveFilePath + toDeleteSceneData.levelName + toDeleteSceneData.levelId + ".png");
		Destroy(toDeleteCard);
		ClosePanel();
	}

	public void ClosePanel()
	{
		toDeleteSceneData = null;
		gameObject.SetActive(false);
	}
}
