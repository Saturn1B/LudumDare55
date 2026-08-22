using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TooltipsCursor : MonoBehaviour
{
    [SerializeField] private GameObject tooltipsPanel;
    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private GameObject scaleIcon;

    void Update()
    {
        tooltipsPanel.transform.position = Input.mousePosition + Vector3.right * 100;
    }

    public void OpenTooltips(string objectName, bool canScale = false)
	{
        tooltipsPanel.SetActive(true);
        objectNameText.text = objectName;
        if (canScale)
            scaleIcon.SetActive(true);
        else
            scaleIcon.SetActive(false);
	}

    public void CloseTooltips()
	{
        tooltipsPanel.SetActive(false);
        scaleIcon.SetActive(false);
    }
}
