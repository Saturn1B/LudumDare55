using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ModifiableObject))]
public class ModifiableObjectEditor : Editor
{
	protected static bool ShowOffsetSettings = false;

	public override void OnInspectorGUI()
	{
		ModifiableObject modifiableObject = (ModifiableObject)target;

		modifiableObject.canTranslate = EditorGUILayout.Toggle("Can Translate", modifiableObject.canTranslate);
		modifiableObject.canScale = EditorGUILayout.Toggle("Can Scale", modifiableObject.canScale);
		modifiableObject.canRotate = EditorGUILayout.Toggle("Can Rotate", modifiableObject.canRotate);

		if (modifiableObject.canRotate)
		{
			EditorGUILayout.Space();

			ShowOffsetSettings = EditorGUILayout.Foldout(ShowOffsetSettings, "Rotation axis");

			if (ShowOffsetSettings)
			{
				modifiableObject.X = EditorGUILayout.Toggle("X", modifiableObject.X);
				modifiableObject.Y = EditorGUILayout.Toggle("Y", modifiableObject.Y);
				modifiableObject.Z = EditorGUILayout.Toggle("Z", modifiableObject.Z);
			}
		}

		// Apply changes made to the script
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}
}
