namespace UI
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count++;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

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
