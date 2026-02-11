using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    public static bool isReady { get; private set; }

	private async void Awake()
	{
		DontDestroyOnLoad(gameObject);

		var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

		if(dependencyStatus == DependencyStatus.Available)
		{
			FirebaseApp app = FirebaseApp.DefaultInstance;
			isReady = true;
			Debug.Log("Firebase Initialized");
		}
		else
		{
			Debug.LogError($"Firebase init failed: {dependencyStatus}");
		}
	}
}
