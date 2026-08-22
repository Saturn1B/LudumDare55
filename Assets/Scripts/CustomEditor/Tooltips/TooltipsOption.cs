using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipsOption : MonoBehaviour
{
    private TooltipsMenu tooltipsMenu;
    [Header("SUBTITLE")]
    public string subtitle;
    [Header("EXPLANATION"), TextArea(10, 10)]
    public string explanation;
    // Start is called before the first frame update
    void Start()
    {
        tooltipsMenu = FindObjectOfType<TooltipsMenu>();
    }

    public void OpenTooltip()
	{
        tooltipsMenu.OpenTooltip(subtitle, explanation);
	}
}
