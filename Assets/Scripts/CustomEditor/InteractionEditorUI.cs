using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InteractionEditorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[SerializeField] private UnityEngine.UI.Button acceptInteractionButton;
	[SerializeField] private UnityEngine.UI.Button acceptDispenserButton;
	public void OnPointerEnter(PointerEventData eventData)
	{
		ObjectSelection.Instance.mouseOverEditorUI = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ObjectSelection.Instance.mouseOverEditorUI = false;
	}

	//private void OnEnable()
	//{
	//	acceptInteractionButton.onClick.RemoveAllListeners();
	//	acceptInteractionButton.onClick.AddListener(() => EditorHUDManager.Instance.CloseInteractionEditor());

	//	acceptDispenserButton.onClick.RemoveAllListeners();
	//	acceptDispenserButton.onClick.AddListener(() => EditorHUDManager.Instance.CloseDispenserEditor());
	//}

	//private void OnDisable()
	//{
	//	acceptInteractionButton.onClick.RemoveAllListeners();
	//	acceptDispenserButton.onClick.RemoveAllListeners();
	//}
}
