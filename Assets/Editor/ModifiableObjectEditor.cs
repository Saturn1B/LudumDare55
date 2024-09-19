using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ModifiableObject))]
public class ModifiableObjectEditor : Editor
{
	protected static bool ShowOffsetSettings = false;

	public override void OnInspectorGUI()
	{
		ModifiableObject modifiableObject = (ModifiableObject)target;

		modifiableObject.isGroundOrWall = EditorGUILayout.Toggle("Is Ground Or Wall Level", modifiableObject.isGroundOrWall);
		modifiableObject.isStuckToWall = EditorGUILayout.Toggle("Is Stuck To Wall", modifiableObject.isStuckToWall);

		EditorGUILayout.Space();

		modifiableObject.canTranslate = EditorGUILayout.Toggle("Can Translate", modifiableObject.canTranslate);

		//if (modifiableObject.canTranslate)
		//{
		//	EditorGUI.indentLevel++;
		//	modifiableObject.translateOffset = EditorGUILayout.Vector3Field("Translate Offset", modifiableObject.translateOffset);
		//	EditorGUI.indentLevel--;
		//	EditorGUILayout.Space();
		//}

		modifiableObject.canScale = EditorGUILayout.Toggle("Can Scale", modifiableObject.canScale);
		modifiableObject.canRotate = EditorGUILayout.Toggle("Can Rotate", modifiableObject.canRotate);

		if (modifiableObject.canRotate)
		{
			EditorGUI.indentLevel++;
			ShowOffsetSettings = EditorGUILayout.Foldout(ShowOffsetSettings, "Rotation axis");

			if (ShowOffsetSettings)
			{
				modifiableObject.X = EditorGUILayout.Toggle("X", modifiableObject.X);
				modifiableObject.Y = EditorGUILayout.Toggle("Y", modifiableObject.Y);
				modifiableObject.Z = EditorGUILayout.Toggle("Z", modifiableObject.Z);
			}
			EditorGUI.indentLevel--;
		}

		// Apply changes made to the script
		if (GUI.changed)
		{
			EditorUtility.SetDirty(target);
		}
	}
}
