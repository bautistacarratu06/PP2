namespace UI
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Rutas de detalle: no aparecen en el flyout, se navega con GoToAsync.
            Routing.RegisterRoute(nameof(Views.StudentDetailPage), typeof(Views.StudentDetailPage));
        }
    }
}
