using MudBlazor;

namespace AppEtiquetado.Components.Layout;

/// <summary>
/// Tema visual alineado con el sistema Cleeny (Primary #1A6868 teal).
/// </summary>
public static class AppTheme
{
    public static readonly MudTheme Default = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1A6868",
            PrimaryDarken = "#155454",
            PrimaryLighten = "#7DDCD6",
            Secondary = "#7B3FA0",
            SecondaryDarken = "#5C2E78",
            Background = "#F5F9F9",
            Surface = "#FFFFFF",
            AppbarBackground = "#1A6868",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#1A6868",
            Success = "#4CAF50",
            Warning = "#FF9800",
            Error = "#D32F2F",
            Info = "#7DDCD6",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#7DDCD6",
            Background = "#0F1E1E",
            Surface = "#162828",
            AppbarBackground = "#162828",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["Roboto", "sans-serif"],
                FontSize = "14px",
            },
            H5 = new H5Typography
            {
                FontFamily = ["Poppins", "Roboto", "sans-serif"],
                FontWeight = "600",
            },
            H6 = new H6Typography
            {
                FontFamily = ["Poppins", "Roboto", "sans-serif"],
                FontWeight = "600",
            },
            Button = new ButtonTypography
            {
                FontFamily = ["Poppins", "Roboto", "sans-serif"],
                FontWeight = "600",
                TextTransform = "none",
            },
        },
    };
}
