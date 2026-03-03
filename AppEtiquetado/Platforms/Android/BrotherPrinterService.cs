using Android.Bluetooth;
using Android.Content;
using AppEtiquetado.Services;
using Com.Brother.Sdk.Lmprinter;
using Com.Brother.Sdk.Lmprinter.Setting;
using Application = Android.App.Application;

namespace AppEtiquetado.Platforms.Android;

public class BrotherPrinterService : IBrotherPrinterService
{
    public List<PrinterDevice> GetPairedPrinters()
    {
        try
        {
            var context = Application.Context;
            var btManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
            var adapter = btManager?.Adapter;

            if (adapter is null || !adapter.IsEnabled)
                return [];

            return (adapter.BondedDevices ?? [])
                .Where(d => !string.IsNullOrEmpty(d.Address))
                .Select(d => new PrinterDevice(d.Name ?? d.Address!, d.Address!))
                .OrderBy(d => d.Name)
                .ToList();
        }
        catch (Exception ex)
        {
            return [new PrinterDevice($"Error: {ex.Message}", "")];
        }
    }

    public async Task<PrintResult> PrintTestAsync(
        string address,
        string labelSize = "DieCutW62H100",
        int labelHeightMm = 50,
        CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var context = Application.Context;
                var btManager = context.GetSystemService(Context.BluetoothService) as BluetoothManager;
                var adapter = btManager?.Adapter;

                var channel = Channel.NewBluetoothChannel(address, adapter);
                var generateResult = PrinterDriverGenerator.OpenChannel(channel);

                if (generateResult.Error.Code != OpenChannelError.ErrorCode.NoError)
                    return new PrintResult(false, $"Error de conexión: {generateResult.Error.Code}");

                var driver = generateResult.Driver;
                try
                {
                    var settings = new QLPrintSettings(PrinterModel.Ql1110nwb);
                    settings.SetLabelSize(ParseLabelSize(labelSize));
                    settings.AutoCut = true;
                    settings.ScaleMode = IPrintImageSettings.ScaleMode.FitPageAspect;
                    settings.WorkPath = context.CacheDir?.AbsolutePath ?? "/data/local/tmp";

                    var bitmap = CreateTestBitmap(labelHeightMm);
                    var printError = driver.PrintImage(bitmap, settings);
                    bitmap.Recycle();

                    var logs = printError.AllLogs is not null
                        ? string.Join("\n", printError.AllLogs.Cast<Java.Lang.Object>().Select(o => o.ToString()))
                        : string.Empty;

                    bool success = printError.Code == PrintError.ErrorCode.NoError;
                    return new PrintResult(success, $"{printError.Code}\n{logs}".Trim());
                }
                finally
                {
                    driver.CloseChannel();
                }
            }
            catch (Exception ex)
            {
                return new PrintResult(false, $"Excepción: {ex.GetType().Name}: {ex.Message}");
            }
        }, ct);
    }

    private static readonly Dictionary<string, QLPrintSettings.LabelSize?> _labelSizeMap = new()
    {
        ["DieCutW62H100"]   = QLPrintSettings.LabelSize.DieCutW62H100,
        ["RollW62"]         = QLPrintSettings.LabelSize.RollW62,
        ["DieCutW103H164"]  = QLPrintSettings.LabelSize.DieCutW103H164,
        ["DieCutW62H29"]    = QLPrintSettings.LabelSize.DieCutW62H29,
        ["DieCutW62H60"]    = QLPrintSettings.LabelSize.DieCutW62H60,
        ["DieCutW102H152"]  = QLPrintSettings.LabelSize.DieCutW102H152,
    };

    private static QLPrintSettings.LabelSize? ParseLabelSize(string name) =>
        _labelSizeMap.TryGetValue(name, out var size) ? size : QLPrintSettings.LabelSize.DieCutW62H100;

    // Empirical factor measured on QL-1110NWB: 200 px → 22.93 mm ≈ 8.725 px/mm
    private const double PxPerMm = 8.725;

    private static global::Android.Graphics.Bitmap CreateTestBitmap(int heightMm)
    {
        const int W = 696;
        int H = Math.Max((int)(heightMm * PxPerMm), 230); // 230 px ≈ 26.4 mm, above the 25.40 mm minimum

        var bmp = global::Android.Graphics.Bitmap.CreateBitmap(
            W, H, global::Android.Graphics.Bitmap.Config.Rgb565!)!;

        using var canvas = new global::Android.Graphics.Canvas(bmp);
        canvas.DrawColor(global::Android.Graphics.Color.White);

        using var paint = new global::Android.Graphics.Paint { AntiAlias = true };
        paint.TextAlign = global::Android.Graphics.Paint.Align.Center;

        paint.Color = global::Android.Graphics.Color.Black;
        paint.TextSize = 64f;
        canvas.DrawText("PRUEBA DE IMPRESIÓN", W / 2f, H * 0.38f, paint);

        paint.TextSize = 40f;
        canvas.DrawText(AppConfig.AppName, W / 2f, H * 0.60f, paint);

        paint.TextSize = 28f;
        paint.Color = global::Android.Graphics.Color.DarkGray;
        canvas.DrawText(DateTime.Now.ToString("dd/MM/yyyy  HH:mm:ss"), W / 2f, H * 0.78f, paint);

        return bmp;
    }
}
