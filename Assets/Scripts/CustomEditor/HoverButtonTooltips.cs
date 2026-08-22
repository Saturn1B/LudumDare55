using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverButtonTooltips : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	private bool overButton;
	private bool isTooltipsDisplayed;
	private ObjectButton objectButton;
	[SerializeField] private string tooltipsText;

	private TooltipsDialog tooltipsDialog;

    private void Start()
    {
		tooltipsDialog = FindObjectOfType<TooltipsDialog>();
		objectButton = GetComponent<ObjectButton>();
    }

	public void OnPointerEnter(PointerEventData eventData)
	{
		overButton = true;
		StartCoroutine(DisplayTooltips());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		HideTooltips();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		HideTooltips();
	}

	private IEnumerator DisplayTooltips()
	{
		yield return new WaitForSeconds(.5f);

		isTooltipsDisplayed = true;
		if(objectButton != null)
			tooltipsDialog.ShowTooltips(objectButton.objectName, transform.position, GetComponent<RectTransform>().sizeDelta.y / 2);
		else
			tooltipsDialog.ShowTooltips(tooltipsText, transform.position, GetComponent<RectTransform>().sizeDelta.y / 2);
	}

	private void HideTooltips()
	{
		overButton = false;
		StopAllCoroutines();

		if (isTooltipsDisplayed)
		{
			isTooltipsDisplayed = false;
			tooltipsDialog.HideTooltips();
		}
	}
}
