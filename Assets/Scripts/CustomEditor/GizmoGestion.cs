using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoGestion : MonoBehaviour
{
	public static GizmoGestion Instance { get; private set; }

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

	[SerializeField] private GameObject positionGizmoObject, scaleGizmoObject, rotationGizmoObject, gizmoCenter;
	[SerializeField] private RotationGizmo rotationGizmo;
	[SerializeField] private ScaleGizmo scaleGizmo;
	[SerializeField] private Transform modifiableObjectParent;

	private ModifiableObject currentModifiableObject;

	public GizmoMode _gizmoMode;

	private void Start()
	{
		DeactivateGizmo();
	}

	public void RefreshGizmoMode(GizmoMode gizmoMode)
	{
		_gizmoMode = gizmoMode;

		if (currentModifiableObject == null) return;

		bool isGizmoSet = false;

		switch (_gizmoMode)
		{
			case GizmoMode.MOVEABLE:
				if (!currentModifiableObject.canTranslate) break;

				positionGizmoObject.SetActive(true);
				scaleGizmoObject.SetActive(false);
				rotationGizmoObject.SetActive(false);

				isGizmoSet = true;
				break;
			case GizmoMode.SCALEABLE:
				if (!currentModifiableObject.canScale) break;

				scaleGizmo.RefreshGizmo();

				positionGizmoObject.SetActive(false);
				scaleGizmoObject.SetActive(true);
				rotationGizmoObject.SetActive(false);

				isGizmoSet = true;
				break;
			case GizmoMode.PIVOTABLE:
				if (!currentModifiableObject.canRotate) break;

				rotationGizmo.allowXRot = currentModifiableObject.X;
				rotationGizmo.allowYRot = currentModifiableObject.Y;
				rotationGizmo.allowZRot = currentModifiableObject.Z;

				rotationGizmo.RefreshGizmo();

				positionGizmoObject.SetActive(false);
				scaleGizmoObject.SetActive(false);
				rotationGizmoObject.SetActive(true);

				isGizmoSet = true;
				break;
		}

		if (!isGizmoSet)
		{
			EditorHUDManager.Instance.SwitchGizmoMode(0);
		}
	}

	public void ActivateGizmo(ModifiableObject modifiableObject)
	{
		if(currentModifiableObject != null)
		{
			if (currentModifiableObject.hasParent)
			currentModifiableObject.transform.SetParent(currentModifiableObject.parentObject.transform);
			else
				currentModifiableObject.transform.SetParent(null);
		}

		currentModifiableObject = modifiableObject;

		transform.position = currentModifiableObject.transform.position;
		currentModifiableObject.transform.SetParent(modifiableObjectParent);

		gizmoCenter.SetActive(true);

		EditorHUDManager.Instance.GizmoSelectionButton(currentModifiableObject.canTranslate, currentModifiableObject.canScale, currentModifiableObject.canRotate);

		bool isGizmoSet = false;

		switch (_gizmoMode)
		{
			case GizmoMode.MOVEABLE:
				if (!currentModifiableObject.canTranslate) break;

				positionGizmoObject.SetActive(true);
				scaleGizmoObject.SetActive(false);
				rotationGizmoObject.SetActive(false);

				isGizmoSet = true;
				break;
			case GizmoMode.SCALEABLE:
				if (!currentModifiableObject.canScale) break;

				scaleGizmo.RefreshGizmo();

				positionGizmoObject.SetActive(false);
				scaleGizmoObject.SetActive(true);
				rotationGizmoObject.SetActive(false);

				isGizmoSet = true;
				break;
			case GizmoMode.PIVOTABLE:
				if (!currentModifiableObject.canRotate) break;

				rotationGizmo.allowXRot = currentModifiableObject.X;
				rotationGizmo.allowYRot = currentModifiableObject.Y;
				rotationGizmo.allowZRot = currentModifiableObject.Z;

				rotationGizmo.RefreshGizmo();

				positionGizmoObject.SetActive(false);
				scaleGizmoObject.SetActive(false);
				rotationGizmoObject.SetActive(true);

				isGizmoSet = true;
				break;
		}

		if (!isGizmoSet)
		{
			EditorHUDManager.Instance.SwitchGizmoMode(0);
		}
	}

	public void DeactivateGizmo()
	{
		Debug.Log("test");
		if (currentModifiableObject)
		{
			if (currentModifiableObject.hasParent)
				currentModifiableObject.transform.SetParent(currentModifiableObject.parentObject.transform);
			else
				currentModifiableObject.transform.SetParent(null);
			currentModifiableObject = null;
		}

		gizmoCenter.SetActive(false);

		positionGizmoObject.SetActive(false);
		scaleGizmoObject.SetActive(false);
		rotationGizmoObject.SetActive(false);
	}
}
