using System.Collections.Generic;
using Firebase.Auth;

public static class FirestoreRestConfig
{
	private const string ProjectId = "matlab-c258c";

	private static string BaseUrl => $"https://firestore.googleapis.com/v1/projects/{ProjectId}/databases/(default)/documents";

	public static string GetDocumentUrl(string collection)
	{
		return $"{BaseUrl}/{collection}";
	}

	public static string GetDocumentUrl(string collection, string documentId)
	{
		return $"{BaseUrl}/{collection}/{documentId}";
	}

	public static Dictionary<string, string> GetAuthHeader(string token)
	{
		return new Dictionary<string, string>
		{
			{ "Authorization", "Bearer " + token }
		};
	}
}
