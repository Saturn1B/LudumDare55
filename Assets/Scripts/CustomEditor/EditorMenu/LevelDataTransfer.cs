using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class LevelDataTransfer
{
	public static SceneData SceneDataToLoad { get; set; }
	public static SceneData SceneDataToTest { get; set; }
	public static string levelName { get; set; }
	public static bool isEditing { get; set; }
}
