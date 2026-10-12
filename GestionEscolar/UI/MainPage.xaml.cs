using GestionEscolar.Domain.Data;

namespace UI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;
        bool isConnected = false;

        public MainPage()
        {
            InitializeComponent();
            CheckDatabaseConnection();
        }

        private void CheckDatabaseConnection()
        {
            var db = DatabaseInstance.GetInstance("MainConnection");
            isConnected = db.TestConnection();

            if (isConnected)
                System.Diagnostics.Debug.WriteLine("Tablas: " + string.Join(", ", db.ListTables()));
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
