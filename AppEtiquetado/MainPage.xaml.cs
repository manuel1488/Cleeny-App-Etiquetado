namespace AppEtiquetado
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            blazorWebView.StartPath = "/login";

#if ANDROID
            // On Android (API 35+ enforces edge-to-edge), push the WebView below
            // the system status bar using the actual measured status bar height.
            var ctx = Android.App.Application.Context;
            var res = ctx.Resources;
            if (res != null)
            {
                int id = res.GetIdentifier("status_bar_height", "dimen", "android");
                float px = id > 0 ? res.GetDimensionPixelSize(id) : 0;
                float dp = px / (res.DisplayMetrics?.Density ?? 1f);
                Padding = new Thickness(0, dp, 0, 0);
            }
#endif
        }
    }
}
