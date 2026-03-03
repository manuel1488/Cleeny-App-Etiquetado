namespace AppEtiquetado.Services;

public interface IBrotherPrinterService
{
    /// <summary>
    /// Devuelve los dispositivos Bluetooth ya emparejados en el sistema.
    /// El usuario debe emparejar la impresora desde Ajustes de Android antes de usar la app.
    /// </summary>
    List<PrinterDevice> GetPairedPrinters();

    /// <summary>
    /// Imprime una etiqueta de prueba en la impresora con la dirección indicada.
    /// </summary>
    Task<PrintResult> PrintTestAsync(
        string address,
        string labelSize = "DK_62X100",
        CancellationToken ct = default);
}

public record PrinterDevice(string Name, string Address);

public record PrintResult(bool Success, string Message);
