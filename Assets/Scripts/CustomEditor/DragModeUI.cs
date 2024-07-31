using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragModeUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		ObjectSelection.Instance.mouseOverDragUI = true;
		ObjectPlacer.Instance.mouseOverDragUI = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ObjectSelection.Instance.mouseOverDragUI = false;
		ObjectPlacer.Instance.mouseOverDragUI = false;
	}
}
