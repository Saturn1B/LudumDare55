using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private GameObject mainPanel, creditsPanel, controlsPanel;
	private GameObject currentPanel;

	private void Start()
	{
		currentPanel = mainPanel;
		currentPanel.SetActive(true);

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;
	}

	public void SwitchPanel(GameObject newPanel)
	{
		currentPanel.SetActive(false);
		currentPanel = newPanel;
		currentPanel.SetActive(true);
	}

	public void BackToMain()
	{
		SwitchPanel(mainPanel);
	}

	public void QuitGame()
	{
		Application.Quit();
	}

	public void PlayGame()
	{
		SceneManager.LoadScene("Level1", LoadSceneMode.Single);
	}


	public void LoadMainMenu()
	{
		SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
	}
}
