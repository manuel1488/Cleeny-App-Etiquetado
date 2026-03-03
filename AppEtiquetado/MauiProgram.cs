using Microsoft.Extensions.Logging;
using MudBlazor;
using MudBlazor.Services;
using AppEtiquetado.Services;
#if ANDROID
using AppEtiquetado.Platforms.Android;
#endif

namespace AppEtiquetado;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

        builder.Services.AddMudServices(config =>
        {
            config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
            config.SnackbarConfiguration.ShowCloseIcon = true;
            config.SnackbarConfiguration.VisibleStateDuration = 4000;
            config.SnackbarConfiguration.PreventDuplicates = true;
        });

        // Servicios de la app (singleton para persistir la sesión y cookies entre páginas)
        builder.Services.AddSingleton<AppApiService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<LabelService>();

        // Impresora Brother (solo Android)
#if ANDROID
        builder.Services.AddSingleton<IBrotherPrinterService, BrotherPrinterService>();
#endif

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
