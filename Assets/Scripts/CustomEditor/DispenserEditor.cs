using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DispenserEditor : MonoBehaviour
{
	[SerializeField] private MeshRenderer mainBar;
	[SerializeField] private MeshRenderer[] stackBars;
	[SerializeField] private Material[] materialsColor;
	[SerializeField] private Material[] materialsColorOff;

	public Materials materialType { get; private set; }
	public int materialNumber { get; private set; }

	private void Start()
	{
		if(materialType == Materials.EMPTY)
		{
			SetMaterial(0);
			SetNumber(0);
		}
	}

	public void SetMaterial(int typeId, bool adder = true)
	{
		Debug.Log($"Test{typeId}");

		if(adder)
			materialType = (Materials)(typeId + 1);
		else
			materialType = (Materials)typeId;

		mainBar.material = materialsColor[(int)materialType - 1];

		SetNumber(materialNumber);
	}

	public void SetNumber(int number)
	{
		materialNumber = number;

		if (number == 0)
			mainBar.material = materialsColorOff[(int)materialType - 1];
		else
			mainBar.material = materialsColor[(int)materialType - 1];

		foreach (var bar in stackBars)
		{
			bar.material = materialsColorOff[(int)materialType - 1];
		}

		for (int i = 0; i < number; i++)
		{
			stackBars[i].material = materialsColor[(int)materialType - 1];
		}
	}
}
