using GestionEscolar.Data;

namespace UI
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseInstance _db;
        int count = 0;
        bool isConnected = false;

        public MainPage(DatabaseInstance db)
        {
            InitializeComponent();
            _db = db;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CheckDatabaseConnectionAsync();
        }

        private async Task CheckDatabaseConnectionAsync()
        {
            try
            {
                isConnected = await _db.TestConnectionAsync();

                if (isConnected)
                    System.Diagnostics.Debug.WriteLine("Tablas: " + string.Join(", ", await _db.ListTablesAsync()));
            }
            catch (Exception ex)
            {
                isConnected = false;
                System.Diagnostics.Debug.WriteLine($"Error al conectar con la base: {ex.Message}");
            }
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time. esta conectado a la base de datos: {isConnected}";
            else
                CounterBtn.Text = $"Clicked {count} times. esta conectado a la base de datos: {isConnected}";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        // TODO: El botón "Panel de seguimiento" de Home llama a esto.
        // Shell.Current.GoToAsync("//TrackingPage") abre la ruta registrada en AppShell.
        // Las // son ruta absoluta: reemplazan Home, no apilan TrackingPage encima.
        private async void OnOpenTrackingClicked(object? sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//StudentListPage");
        }
    }
}
