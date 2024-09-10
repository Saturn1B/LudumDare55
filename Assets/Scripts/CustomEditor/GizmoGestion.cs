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

		switch (_gizmoMode)
		{
			case GizmoMode.MOVEABLE:
				if (!currentModifiableObject.canTranslate) break;

				positionGizmoObject.SetActive(true);
				scaleGizmoObject.SetActive(false);
				rotationGizmoObject.SetActive(false);
				break;
			case GizmoMode.SCALEABLE:
				if (!currentModifiableObject.canScale) break;

				positionGizmoObject.SetActive(false);
				scaleGizmoObject.SetActive(true);
				rotationGizmoObject.SetActive(false);
				break;
			case GizmoMode.PIVOTABLE:
				if (!currentModifiableObject.canRotate) break;

				rotationGizmo.allowXRot = currentModifiableObject.X;
				rotationGizmo.allowYRot = currentModifiableObject.Y;
				rotationGizmo.allowZRot = currentModifiableObject.Z;

				positionGizmoObject.SetActive(false);
				scaleGizmoObject.SetActive(false);
				rotationGizmoObject.SetActive(true);
				break;
		}
	}

	public void ActivateGizmo(ModifiableObject modifiableObject)
	{
		if(currentModifiableObject != null)
		{
			currentModifiableObject.transform.SetParent(null);
		}

		currentModifiableObject = modifiableObject;

		transform.position = currentModifiableObject.transform.position;
		currentModifiableObject.transform.SetParent(modifiableObjectParent);

		gizmoCenter.SetActive(true);

		switch (_gizmoMode)
		{
			case GizmoMode.MOVEABLE:
				if (!currentModifiableObject.canTranslate) break;

				positionGizmoObject.SetActive(true);
				scaleGizmoObject.SetActive(false);
				rotationGizmoObject.SetActive(false);
				break;
			case GizmoMode.SCALEABLE:
				if (!currentModifiableObject.canScale) break;

				positionGizmoObject.SetActive(false);
				scaleGizmoObject.SetActive(true);
				rotationGizmoObject.SetActive(false);
				break;
			case GizmoMode.PIVOTABLE:
				if (!currentModifiableObject.canRotate) break;

				rotationGizmo.allowXRot = currentModifiableObject.X;
				rotationGizmo.allowYRot = currentModifiableObject.Y;
				rotationGizmo.allowZRot = currentModifiableObject.Z;

				positionGizmoObject.SetActive(false);
				scaleGizmoObject.SetActive(false);
				rotationGizmoObject.SetActive(true);
				break;
		}
	}

	public void DeactivateGizmo()
	{
		if (currentModifiableObject)
		{
			currentModifiableObject.transform.SetParent(null);
			currentModifiableObject = null;
		}

		gizmoCenter.SetActive(false);

		positionGizmoObject.SetActive(false);
		scaleGizmoObject.SetActive(false);
		rotationGizmoObject.SetActive(false);
	}
}
