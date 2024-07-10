using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public enum MouseDragMode
{
	MOVEABLE = 0,
	SCALEABLE = 1,
	PIVOTABLE = 2
}

public class EditorHUDManager : MonoBehaviour
{
	public static EditorHUDManager Instance { get; private set; }

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			Destroy(this);
		}
		else
		{
			Instance = this;
		}
	}

	private void Start()
	{
		SwitchMouseDragMode((int)MouseDragMode.MOVEABLE);
	}

	private MouseDragMode _mouseDragMode;
	[SerializeField] private GameObject[] mouseDragModeImages;

	public void SwitchMouseDragMode(int mode)
	{
		switch ((MouseDragMode)mode)
		{
			case MouseDragMode.MOVEABLE:
				ObjectRegister.Instance.SetMoveableMode();
				break;
			case MouseDragMode.SCALEABLE:
				ObjectRegister.Instance.SetScaleableMode();
				break;
			case MouseDragMode.PIVOTABLE:
				break;
			default:
				break;
		}

		for (int i = 0; i < mouseDragModeImages.Length; i++)
		{
			if (i == (int)mode)
				mouseDragModeImages[i].SetActive(true);
			else
				mouseDragModeImages[i].SetActive(false);
		}
	}
}
