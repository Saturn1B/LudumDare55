using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActivatorEditor : MonoBehaviour
{
	[HideInInspector] public string activatorName;

	/*[HideInInspector]*/ public List<ActivableEditor> activables = new List<ActivableEditor>();

	[SerializeField] private GameObject warningSign;

	private void Start()
	{
		if (activables.Count > 0)
			SwitchWarningSignState(false);
		else
			SwitchWarningSignState(true);
	}

	public void RemoveActivable(ActivableEditor activable)
	{
		activables.Remove(activable);
	}

	public void RemoveFromActivable()
	{
		foreach (var activable in activables)
		{
			activable.RemoveActivator(this);
		}
	}

	public void SwitchWarningSignState(bool warningState)
	{
		warningSign.SetActive(warningState);
	}
}
