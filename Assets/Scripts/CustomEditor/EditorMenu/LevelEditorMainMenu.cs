using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

public class LevelEditorMainMenu : MonoBehaviour
{
	[SerializeField] private GameObject levelCardPrefab;
	[SerializeField] private Transform cardParent;

	[SerializeField] private TMP_Text myLevelsText, onlineLevelsText, sectionTitleText;
	[SerializeField] private string section1, section2;
	[SerializeField] private Color highlightButtonColor;

	[SerializeField] private GameObject createLevelPopup, deleteLevelPopup;


	private void Awake()
	{
		SaveSystem.GenerateSaveFilePath();
	}

	private void OnEnable()
	{
		ActivateMyLevels();
	}

	private void OnDisable()
	{

	}

	public void ActivateMyLevels()
	{
		ClearLevelContainer();

		sectionTitleText.text = $"{section1} <";

		myLevelsText.text = $"-> {section1}";
		myLevelsText.color = highlightButtonColor;
		onlineLevelsText.text = $"> {section2}";
		onlineLevelsText.color = Color.white;

		GameObject cc = Instantiate(levelCardPrefab, cardParent);
		cc.GetComponent<LevelCardMenu>().SetCard(LevelCardType.CREATE, null, null, OpenCreateLevelPopup);

		if (!Directory.Exists(SaveSystem.saveFilePath)) return;

		string[] jsonLevelDataFiles = Directory.GetFiles(SaveSystem.saveFilePath, "*.json");

		List<SceneData> levelsData = new List<SceneData>();

		foreach (string file in jsonLevelDataFiles)
		{
			try
			{
				string levelDataString = File.ReadAllText(file);
				SceneData levelData = JsonUtility.FromJson<SceneData>(levelDataString);
				levelsData.Add(levelData);

				GameObject go = Instantiate(levelCardPrefab, cardParent);
				go.GetComponent<LevelCardMenu>().SetCard(LevelCardType.EDIT, levelData, OpenDeleteLevelPopup);
			}
			catch (IOException ex)
			{
				Debug.LogError($"Failed to read file: {file}. Error: {ex.Message}");
			}
		}
	}

	public void ActivateOnlineLevels()
	{
		ClearLevelContainer();

		sectionTitleText.text = $"{section2} <";

		myLevelsText.text = $"> {section1}";
		myLevelsText.color = Color.white;
		onlineLevelsText.text = $"-> {section2}";
		onlineLevelsText.color = highlightButtonColor;
	}

	private void ClearLevelContainer()
	{
		foreach (Transform child in cardParent.transform)
		{
			Destroy(child.gameObject);
		}
	}

	public void OpenCreateLevelPopup() => createLevelPopup.SetActive(true);
	public void OpenDeleteLevelPopup(SceneData sceneData, GameObject levelCard)
	{
		deleteLevelPopup.SetActive(true);
		deleteLevelPopup.GetComponent<LevelDeletion>().Setup(sceneData, levelCard);
	}
}
