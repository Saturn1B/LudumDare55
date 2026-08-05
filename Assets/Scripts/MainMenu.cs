using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	[SerializeField] private GameObject mainPanel, creditsPanel, controlsPanel, levelEditorPanel, accountPanel;
	[SerializeField] private TMPro.TMP_Text username, status;
	[SerializeField] private GameObject connectingIndicator;
	private GameObject currentPanel;

	private void Start()
	{
		currentPanel = mainPanel;
		currentPanel.SetActive(true);

		if (AuthenticationManager.IsSignedIn && !string.IsNullOrEmpty(AuthenticationManager.CurrentUsername))
		{
			accountPanel.SetActive(true);
			username.text = AuthenticationManager.CurrentUsername;
		}

		Cursor.lockState = CursorLockMode.None;
		Cursor.visible = true;

		for (int i = 0; i < 4; i++)
		{
			if (PlayerPrefs.HasKey($"Level{i + 1}"))
			{
				PlayerPrefs.SetInt($"Level{i + 1}", 0);
			}
		}
	}

	public async void SwitchPanel(GameObject newPanel)
	{

		if (newPanel == levelEditorPanel)
		{
			currentPanel.SetActive(false);

			SetBusy(true);
			status.gameObject.SetActive(true);
			status.text = "CONNECTING";

			bool success = await AuthenticationManager.Instance.EnsureGoogleSignedInAsync();

			SetBusy(false);

			if (success)
			{
				username.text = await AuthenticationManager.Instance.RefreshUsernameAsync();

				status.gameObject.SetActive(false);
				currentPanel = newPanel;
				currentPanel.SetActive(true);
				accountPanel.SetActive(true);
			}
			else
			{
				status.text = "<color=red>FAILED CONNECTION</color>";
				await Task.Delay(2500);
				status.gameObject.SetActive(false);
				currentPanel.SetActive(true);
			}

			return;
		}

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

	public void LogOut()
	{
		accountPanel.SetActive(false);
		AuthenticationManager.Instance.SignOut();

		if (currentPanel == levelEditorPanel)
			SwitchPanel(mainPanel);
	}

	private void SetBusy(bool busy)
	{
		if (connectingIndicator != null)
			connectingIndicator.SetActive(busy);
	}
}
