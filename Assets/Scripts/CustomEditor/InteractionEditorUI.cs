using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractionEditorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private UnityEngine.UI.Button acceptButton;

	public void OnPointerEnter(PointerEventData eventData)
	{
		ObjectSelection.Instance.mouseOverEditorUI = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ObjectSelection.Instance.mouseOverEditorUI = false;
	}

	private void OnEnable()
	{
		acceptButton.onClick.RemoveAllListeners();
		acceptButton.onClick.AddListener(() => EditorHUDManager.Instance.CloseInteractionEditor());
	}

	private void OnDisable()
	{
		acceptButton.onClick.RemoveAllListeners();
	}
}
