using Android.Bluetooth;
using Android.Content;
using AppEtiquetado.Models;
using AppEtiquetado.Services;
using Com.Brother.Sdk.Lmprinter;
using Com.Brother.Sdk.Lmprinter.Setting;
using ZXing;
using ZXing.Common;
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

    public async Task<PrintResult> PrintLabelAsync(
        BulkLabelJobDto job,
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

                    var bitmap = CreateLabelBitmap(job, labelHeightMm);
                    var printError = driver.PrintImage(bitmap, settings);
                    bitmap.Recycle();

                    bool success = printError.Code == PrintError.ErrorCode.NoError;
                    var logs = printError.AllLogs is not null
                        ? string.Join("\n", printError.AllLogs.Cast<Java.Lang.Object>().Select(o => o.ToString()))
                        : string.Empty;
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

    // Empirical factor measured on QL-1110NWB: 200 px → 22.93 mm ≈ 8.725 px/mm
    private const double PxPerMm = 8.725;

    private static global::Android.Graphics.Bitmap CreateLabelBitmap(BulkLabelJobDto job, int heightMm)
    {
        const int W = 696;
        int H = Math.Max((int)(heightMm * PxPerMm), 230);
        const int margin = 20;

        var bmp = global::Android.Graphics.Bitmap.CreateBitmap(
            W, H, global::Android.Graphics.Bitmap.Config.Rgb565!)!;
        using var canvas = new global::Android.Graphics.Canvas(bmp);
        canvas.DrawColor(global::Android.Graphics.Color.White);

        using var paint = new global::Android.Graphics.Paint { AntiAlias = true };

        // Product name (bold, scaled to fit)
        paint.SetTypeface(global::Android.Graphics.Typeface.DefaultBold);
        paint.Color = global::Android.Graphics.Color.Black;
        paint.TextSize = 56f;
        DrawTextFitted(canvas, paint, job.ProductName.ToUpperInvariant(), margin, W - margin, (int)(H * 0.20f));

        // Product code
        paint.SetTypeface(global::Android.Graphics.Typeface.Default);
        paint.TextSize = 34f;
        paint.Color = global::Android.Graphics.Color.DarkGray;
        canvas.DrawText(job.ProductCode, margin, (int)(H * 0.34f), paint);

        // Quantity + unit + total price
        paint.TextSize = 44f;
        paint.Color = global::Android.Graphics.Color.Black;
        canvas.DrawText(
            $"{job.Quantity:0.###} {job.UnitMeasureCode}   ${job.TotalPrice:0.00}",
            margin, (int)(H * 0.50f), paint);

        // Separator
        paint.StrokeWidth = 2f;
        canvas.DrawLine(margin, (int)(H * 0.55f), W - margin, (int)(H * 0.55f), paint);

        // Barcode
        int barcodeTop = (int)(H * 0.57f);
        int barcodeH = (int)(H * 0.34f);
        using var barcodeBmp = CreateBarcodeBitmap(job, W - 2 * margin, barcodeH);
        canvas.DrawBitmap(barcodeBmp, margin, barcodeTop, null);

        // Human-readable barcode text
        paint.TextSize = 20f;
        paint.Color = global::Android.Graphics.Color.Gray;
        paint.TextAlign = global::Android.Graphics.Paint.Align.Center;
        canvas.DrawText(
            $"{job.ProductCode} | {job.Quantity:0.###} {job.UnitMeasureCode} | ${job.TotalPrice:0.00}",
            W / 2f, (int)(H * 0.96f), paint);

        return bmp;
    }

    private static global::Android.Graphics.Bitmap CreateBarcodeBitmap(
        BulkLabelJobDto job, int width, int height)
    {
        var qtyMillis = ((long)Math.Round(job.Quantity * 1000)).ToString("D6");
        var priceCents = ((long)Math.Round(job.TotalPrice * 100)).ToString();
        var content = $"{job.ProductCode}|{qtyMillis}|{priceCents}";

        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions
            {
                Width = width,
                Height = height,
                Margin = 4,
                PureBarcode = true
            }
        };

        var pixelData = writer.Write(content);

        // ZXing returns BGRA; Android.Graphics.Bitmap.CreateBitmap(int[]) expects ARGB packed ints
        var argb = new int[pixelData.Width * pixelData.Height];
        for (int i = 0; i < argb.Length; i++)
        {
            byte b = pixelData.Pixels![i * 4];
            byte g = pixelData.Pixels![i * 4 + 1];
            byte r = pixelData.Pixels![i * 4 + 2];
            byte a = pixelData.Pixels![i * 4 + 3];
            argb[i] = (a << 24) | (r << 16) | (g << 8) | b;
        }

        return global::Android.Graphics.Bitmap.CreateBitmap(
            argb, pixelData.Width, pixelData.Height,
            global::Android.Graphics.Bitmap.Config.Argb8888!)!;
    }

    private static void DrawTextFitted(
        global::Android.Graphics.Canvas canvas,
        global::Android.Graphics.Paint paint,
        string text, int x, int maxX, int y)
    {
        float available = maxX - x;
        float measured = paint.MeasureText(text);
        if (measured > available)
        {
            float original = paint.TextSize;
            paint.TextSize = original * available / measured;
            canvas.DrawText(text, x, y, paint);
            paint.TextSize = original;
        }
        else
        {
            canvas.DrawText(text, x, y, paint);
        }
    }

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
