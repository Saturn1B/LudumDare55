using System.IO;
using UnityEngine;
using Newtonsoft.Json;

[System.Serializable]
public class GoogleOAuthConfig
{
    public string ClientId;
    public string ClientSecret;

    private static GoogleOAuthConfig _cached;

    public static GoogleOAuthConfig Load()
    {
        if (_cached != null)
            return _cached;

        string path = Path.Combine(Application.streamingAssetsPath, "google-oauth-desktop.json");

        if (!File.Exists(path))
            throw new FileNotFoundException(
                $"Missing {path}. Copy google-oauth-desktop.json.example, rename it, " +
                "and fill in your OAuth Desktop client credentials.");

        _cached = JsonConvert.DeserializeObject<GoogleOAuthConfig>(File.ReadAllText(path));
        return _cached;
    }
}