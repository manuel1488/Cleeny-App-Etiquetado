namespace AppEtiquetado
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            blazorWebView.StartPath = "/conexion-impresora";
        }
    }
}
