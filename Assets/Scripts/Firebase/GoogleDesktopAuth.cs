using System;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public static class GoogleDesktopAuth
{
    // From Google Cloud Console > Credentials > OAuth client ID > Desktop app
    private static string ClientId => GoogleOAuthConfig.Load().ClientId;
    private static string ClientSecret => GoogleOAuthConfig.Load().ClientSecret;

    public class GoogleTokens
    {
        public string IdToken;
        public string AccessToken;
        public string RefreshToken;
        public string DisplayName;
        public string Email;
    }

    public static async Task<GoogleTokens> SignInAsync()
    {
        string codeVerifier = GenerateCodeVerifier();
        string codeChallenge = GenerateCodeChallenge(codeVerifier);
        string state = Guid.NewGuid().ToString("N");

        int port = GetFreePort();
        string redirectUri = $"http://127.0.0.1:{port}/";

        using var listener = new HttpListener();
        listener.Prefixes.Add(redirectUri);
        listener.Start();

        string authUrl =
            "https://accounts.google.com/o/oauth2/v2/auth" +
            $"?client_id={Uri.EscapeDataString(ClientId)}" +
            $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
            "&response_type=code" +
            $"&scope={Uri.EscapeDataString("openid email profile")}" +
            "&access_type=offline" +      // <- required to get a refresh_token back
            $"&state={state}" +
            $"&code_challenge={codeChallenge}" +
            "&code_challenge_method=S256";

        Application.OpenURL(authUrl);

        string code = await WaitForAuthorizationCode(listener, state);
        listener.Stop();

        if (string.IsNullOrEmpty(code))
            throw new Exception("Google sign-in was cancelled or failed.");

        var form = new WWWForm();
        form.AddField("code", code);
        form.AddField("client_id", ClientId);
        form.AddField("client_secret", ClientSecret);
        form.AddField("redirect_uri", redirectUri);
        form.AddField("grant_type", "authorization_code");
        form.AddField("code_verifier", codeVerifier);

        return await PostTokenRequest(form);
    }

    /// Silently redeem a stored refresh_token for a fresh id_token - no browser needed.
    public static async Task<GoogleTokens> RefreshTokensAsync(string refreshToken)
    {
        var form = new WWWForm();
        form.AddField("refresh_token", refreshToken);
        form.AddField("client_id", ClientId);
        form.AddField("client_secret", ClientSecret);
        form.AddField("grant_type", "refresh_token");

        var tokens = await PostTokenRequest(form);

        // Refresh responses usually don't include a new refresh_token (it doesn't rotate) -
        // keep using the one we already had in that case.
        if (string.IsNullOrEmpty(tokens.RefreshToken))
            tokens.RefreshToken = refreshToken;

        return tokens;
    }

    private static async Task<GoogleTokens> PostTokenRequest(WWWForm form)
    {
        using UnityWebRequest request = UnityWebRequest.Post("https://oauth2.googleapis.com/token", form);
        var op = request.SendWebRequest();
        while (!op.isDone) await Task.Yield();

        if (request.result != UnityWebRequest.Result.Success)
            throw new Exception($"Token request failed: {request.error} | {request.downloadHandler.text}");

        var json = JObject.Parse(request.downloadHandler.text);
        string idToken = json["id_token"]?.ToString();
        var claims = DecodeIdTokenClaims(idToken);

        return new GoogleTokens
        {
            IdToken = idToken,
            AccessToken = json["access_token"]?.ToString(),
            RefreshToken = json["refresh_token"]?.ToString(),
            DisplayName = claims?["name"]?.ToString(),
            Email = claims?["email"]?.ToString()
        };
    }

    // Reads the "name"/"email" claims straight out of the ID token. We don't need to
    // verify the signature ourselves - it came directly from Google's token endpoint
    // over HTTPS, and Firebase verifies it again server-side when we sign in with it.
    private static JObject DecodeIdTokenClaims(string idToken)
    {
        if (string.IsNullOrEmpty(idToken)) return null;

        string[] parts = idToken.Split('.');
        if (parts.Length < 2) return null;

        string payload = parts[1].Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        byte[] bytes = Convert.FromBase64String(payload);
        return JObject.Parse(Encoding.UTF8.GetString(bytes));
    }

    private static async Task<string> WaitForAuthorizationCode(HttpListener listener, string expectedState)
    {
        var context = await listener.GetContextAsync();
        var query = context.Request.QueryString;

        string code = query["code"];
        string state = query["state"];
        string error = query["error"];

        string html = error == null
            ? "<html><body>Signed in! You can close this window.</body></html>"
            : $"<html><body>Sign-in failed: {error}</body></html>";

        byte[] buffer = Encoding.UTF8.GetBytes(html);
        context.Response.ContentLength64 = buffer.Length;
        await context.Response.OutputStream.WriteAsync(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();

        if (error != null || state != expectedState)
            return null;

        return code;
    }

    private static int GetFreePort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        int port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static string GenerateCodeVerifier()
    {
        byte[] bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Base64UrlEncode(bytes);
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        using var sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(Encoding.ASCII.GetBytes(codeVerifier));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
}