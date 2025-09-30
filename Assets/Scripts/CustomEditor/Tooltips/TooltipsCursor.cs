using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipsCursor : MonoBehaviour
{
    [SerializeField] private GameObject tooltipsPanel;
    [SerializeField] private TMP_Text objectNameText;

    void Update()
    {
        tooltipsPanel.transform.position = Input.mousePosition + Vector3.right * 100;
    }

    public void OpenTooltips(string objectName)
	{
        tooltipsPanel.SetActive(true);
        objectNameText.text = objectName;
	}

    public void CloseTooltips()
	{
        tooltipsPanel.SetActive(false);
    }
}
