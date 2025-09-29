using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ActivableEditor : MonoBehaviour
{
	[HideInInspector] public string activableName;
	[HideInInspector] public bool needPower;

	/*[HideInInspector]*/ public List<ActivatorEditor> activators = new List<ActivatorEditor>();

	[SerializeField] private GameObject warningSign;

	private void Start()
	{
		if(activators.Count > 0)
			SwitchWarningSignState(false);
		else
			SwitchWarningSignState(true);
	}

	public void RemoveActivator(ActivatorEditor activator)
	{
		activators.Remove(activator);
	}

	public void RemoveFromActivator()
	{
		foreach (var activator in activators)
		{
			activator.RemoveActivable(this);
		}
	}

	public void SwitchWarningSignState(bool warningState)
	{
		warningSign.SetActive(warningState);
	}
}
