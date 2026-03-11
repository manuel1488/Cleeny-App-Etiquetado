using System.Net;

namespace AppEtiquetado.Services;

/// <summary>
/// Envuelve el HttpClient con manejo de cookies para autenticación
/// contra el servidor Cleeny (cookie-based ASP.NET Core Identity).
/// </summary>
public class AppApiService
{
    private CookieContainer _cookieContainer = new();
    private HttpClient _client;
    private string _baseUrl = string.Empty;

    public AppApiService()
    {
        _client = CreateClient(_cookieContainer);
    }

    public HttpClient Client => _client;
    public string BaseUrl => _baseUrl;

    public void SetBaseUrl(string url)
    {
        _baseUrl = url.TrimEnd('/');
        var newUri = new Uri(_baseUrl + "/");

        try
        {
            _client.BaseAddress = newUri;
        }
        catch (InvalidOperationException)
        {
            // HttpClient already sent requests — recreate it preserving cookies
            _client.Dispose();
            _client = CreateClient(_cookieContainer);
            _client.BaseAddress = newUri;
        }
    }

    public void ResetCookies()
    {
        _cookieContainer = new CookieContainer();
        var previous = _client.BaseAddress;
        _client.Dispose();
        _client = CreateClient(_cookieContainer);
        if (previous is not null)
            _client.BaseAddress = previous;
    }

    private static HttpClient CreateClient(CookieContainer cookies)
    {
        var handler = new HttpClientHandler
        {
            CookieContainer = cookies,
            UseCookies = true,
            AllowAutoRedirect = false,
#if DEBUG
            // Permite certificados de desarrollo auto-firmados
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
#endif
        };
        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30),
        };
    }
}
