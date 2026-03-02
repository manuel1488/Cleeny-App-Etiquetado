using System.Net.Http.Json;
using System.Text.Json;
using AppEtiquetado.Models;

namespace AppEtiquetado.Services;

public class LabelService
{
    private readonly AppApiService _api;

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    public LabelService(AppApiService api)
    {
        _api = api;
    }

    /// <summary>
    /// Busca productos activos en Cleeny.
    /// Endpoint esperado: GET /api/products?search={term}&pageSize=20&isActive=true
    /// Soporta respuesta como array directo o como PagedResult{ProductDto}.
    /// </summary>
    public async Task<List<ProductDto>> SearchProductsAsync(string search, CancellationToken ct = default)
    {
        var url = $"/api/products?search={Uri.EscapeDataString(search)}&pageSize=20&isActive=true";
        var response = await _api.Client.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        using var doc = JsonDocument.Parse(json);

        // Respuesta como array directo: [...]
        if (doc.RootElement.ValueKind == JsonValueKind.Array)
            return JsonSerializer.Deserialize<List<ProductDto>>(json, _jsonOptions) ?? [];

        // Respuesta paginada: { "items": [...], "totalCount": N }
        if (doc.RootElement.TryGetProperty("items", out var items))
            return JsonSerializer.Deserialize<List<ProductDto>>(items.GetRawText(), _jsonOptions) ?? [];

        return [];
    }

    /// <summary>
    /// Crea un trabajo de etiquetado y persiste en Cleeny.
    /// Endpoint: POST /api/labels/bulk
    /// </summary>
    public async Task<BulkLabelJobDto> CreateLabelJobAsync(CreateBulkLabelJobDto dto, CancellationToken ct = default)
    {
        var response = await _api.Client.PostAsJsonAsync("/api/labels/bulk", dto, _jsonOptions, ct);
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<BulkLabelJobDto>(_jsonOptions, ct))!;
    }

    /// <summary>
    /// URL directa del PDF de la etiqueta para abrir en visor/impresora.
    /// </summary>
    public string GetPdfUrl(long jobId) => $"{_api.BaseUrl}/api/labels/bulk/{jobId}/pdf";
}
