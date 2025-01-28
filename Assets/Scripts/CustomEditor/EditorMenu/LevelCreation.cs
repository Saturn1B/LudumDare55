using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelCreation : MonoBehaviour
{
	[SerializeField] private TMP_InputField levelNameInput;
	[SerializeField] private TMP_Text nameError;

	private void OnEnable()
	{
		levelNameInput.text = "";
	}

	public void CreateLevel()
	{
		if (string.IsNullOrEmpty(levelNameInput.text))
		{
			StartCoroutine(DisplayNameError());
			return;
		}

		LevelDataTransfer.SceneDataToLoad = null;
		LevelDataTransfer.levelName = levelNameInput.text;
		SceneManager.LoadScene("EditorScene", LoadSceneMode.Single);
	}

	public void ClosePanel() => gameObject.SetActive(false);

	private IEnumerator DisplayNameError()
	{
		nameError.gameObject.SetActive(true);
		nameError.color = new Color(nameError.color.r, nameError.color.g, nameError.color.b, 0);

		float a = 0;

		while (a < 1)
		{
			a += .01f;
			nameError.color += new Color(0, 0, 0, .01f);
			yield return null;
		}

		a = 1;

		yield return new WaitForSecondsRealtime(2);

		while (a > 0)
		{
			a -= .01f;
			nameError.color -= new Color(0, 0, 0, .01f);
			yield return null;
		}

		nameError.gameObject.SetActive(false);
	}
}
