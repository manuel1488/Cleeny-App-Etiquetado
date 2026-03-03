namespace AppEtiquetado.Services;

public interface IBrotherPrinterService
{
    /// <summary>
    /// Returns Bluetooth devices already paired on the system.
    /// The user must pair the printer from Android Settings before using the app.
    /// </summary>
    List<PrinterDevice> GetPairedPrinters();

    /// <summary>
    /// Prints a test label on the printer at the given address.
    /// </summary>
    Task<PrintResult> PrintTestAsync(
        string address,
        string labelSize = "DieCutW62H100",
        int labelHeightMm = 50,
        CancellationToken ct = default);
}

public record PrinterDevice(string Name, string Address);

public record PrintResult(bool Success, string Message);
