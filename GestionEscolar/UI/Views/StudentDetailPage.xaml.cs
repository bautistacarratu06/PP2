using UI.ViewModels;

namespace UI.Views
{
    public partial class StudentDetailPage : ContentPage
    {
        public StudentDetailPage(StudentDetailViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        // TODO: abrir el formulario de intervención cuando exista.
        private async void OnRegisterContactClicked(object? sender, EventArgs e)
        {
            await DisplayAlertAsync("Registrar contacto", "Todavía no está implementado.", "OK");
        }
    }
}
