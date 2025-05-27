using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class InteractableLister : MonoBehaviour
{
	[SerializeField] private TMP_Text objectName;

	[SerializeField] private UnityEngine.UI.Button removeButton;
	[SerializeField] private GameObject highlightImage;

	[HideInInspector] public ActivableEditor activableEditor;
	[HideInInspector] public ActivatorEditor activatorEditor;

	public void Setup(ActivableEditor activableEditor)
	{
		this.activableEditor = activableEditor;
		objectName.text = activableEditor.activableName;

		//HighlightActivable();
	}
	public void Setup(ActivatorEditor activatorEditor)
	{
		this.activatorEditor = activatorEditor;
		objectName.text = activatorEditor.activatorName;

		removeButton.gameObject.SetActive(false);

		//HighlightActivable();
	}

	public void RemoveHighlight()
	{
		highlightImage.SetActive(false);

		if(activableEditor != null)
		{
			if (activableEditor.gameObject.GetComponent<Outline>() != null)
			{
				activableEditor.gameObject.GetComponent<Outline>().enabled = false;
			}
		}
		else
		{
			if (activatorEditor != null && activatorEditor.gameObject.GetComponent<Outline>() != null)
			{
				activatorEditor.gameObject.GetComponent<Outline>().enabled = false;
			}
		}
	}

	public void HighlightActivable()
	{
		EditorHUDManager.Instance.HideAllCurrentActivableHighlight();
		highlightImage.SetActive(true);

		if (activableEditor != null)
		{
			if (activableEditor.gameObject.GetComponent<Outline>() != null)
			{
				activableEditor.gameObject.GetComponent<Outline>().enabled = true;
			}
			else
			{
				Outline outline = activableEditor.gameObject.AddComponent<Outline>();
				outline.enabled = true;
				activableEditor.gameObject.GetComponent<Outline>().OutlineColor = Color.cyan;
				activableEditor.gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
			}
		}
		else
		{
			if (activatorEditor.gameObject.GetComponent<Outline>() != null)
			{
				activatorEditor.gameObject.GetComponent<Outline>().enabled = true;
			}
			else
			{
				Outline outline = activatorEditor.gameObject.AddComponent<Outline>();
				outline.enabled = true;
				activatorEditor.gameObject.GetComponent<Outline>().OutlineColor = Color.cyan;
				activatorEditor.gameObject.GetComponent<Outline>().OutlineWidth = 7.0f;
			}
		}


	}

	private void OnEnable()
	{
		if(activatorEditor != null)
			return;

		removeButton.onClick.RemoveAllListeners();
		removeButton.onClick.AddListener(() =>
		{
			if (activableEditor.gameObject.GetComponent<Outline>() != null)
				activableEditor.gameObject.GetComponent<Outline>().enabled = false;
			EditorHUDManager.Instance.RemoveActivableInteractionEditor(this.activableEditor);
		});
	}

	private void OnDisable()
	{
		removeButton.onClick.RemoveAllListeners();
	}
}
