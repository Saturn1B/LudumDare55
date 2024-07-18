using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectSelecterUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public void OnPointerEnter(PointerEventData eventData)
	{
		ObjectPlacer.Instance.mouseOverSelecterUI = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ObjectPlacer.Instance.mouseOverSelecterUI = false;
	}
}
