using System.Net;
using AppEtiquetado.Models;

namespace AppEtiquetado.Services;

public class AuthService
{
    private readonly AppApiService _api;

    public bool IsAuthenticated { get; private set; }
    public string UserName { get; private set; } = string.Empty;
    public string ServerUrl { get; private set; } = string.Empty;

    public AuthService(AppApiService api)
    {
        _api = api;
        ServerUrl = Preferences.Get("server_url", "https://sistema.cleeny.com.mx");
        if (!string.IsNullOrEmpty(ServerUrl))
            _api.SetBaseUrl(ServerUrl);
    }

    /// <summary>
    /// Autentica contra el endpoint POST /api/Auth/Login de Cleeny.
    /// Cleeny usa cookie-based Identity: responde con 302 a "/" en éxito
    /// o 302 a "/Account/Login?error=..." en fallo.
    /// </summary>
    public async Task<LoginResult> LoginAsync(string serverUrl, string username, string password)
    {
        _api.SetBaseUrl(serverUrl);

        var form = new FormUrlEncodedContent(
        [
            new("Input.UserName", username),
            new("Input.Password", password),
            new("Input.RememberMe", "false"),
        ]);

        HttpResponseMessage response;
        try
        {
            response = await _api.Client.PostAsync("/api/Auth/Login", form);
        }
        catch (Exception ex)
        {
            return new LoginResult(false, $"No se pudo conectar: {ex.Message}");
        }

        // 302/301 redirect → verificar destino
        if (response.StatusCode is HttpStatusCode.Redirect or HttpStatusCode.MovedPermanently)
        {
            var location = response.Headers.Location?.ToString() ?? string.Empty;
            if (location.Contains("error", StringComparison.OrdinalIgnoreCase))
                return new LoginResult(false, "Usuario o contraseña incorrectos.");

            return await SetAuthenticatedAsync(serverUrl, username);
        }

        if (response.IsSuccessStatusCode)
            return await SetAuthenticatedAsync(serverUrl, username);

        return new LoginResult(false, $"Error del servidor ({(int)response.StatusCode}).");
    }

    public async Task LogoutAsync()
    {
        try { await _api.Client.PostAsync("/Account/Logout", null); }
        catch { /* ignorar errores de red al cerrar sesión */ }
        finally
        {
            IsAuthenticated = false;
            UserName = string.Empty;
            _api.ResetCookies();
        }
    }

    private Task<LoginResult> SetAuthenticatedAsync(string serverUrl, string username)
    {
        Preferences.Set("server_url", serverUrl);
        ServerUrl = serverUrl;
        IsAuthenticated = true;
        UserName = username;
        return Task.FromResult(new LoginResult(true));
    }
}

public record LoginResult(bool Success, string? Error = null);
