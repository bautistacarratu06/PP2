using UI.ViewModels;

namespace UI.Views
{
    public partial class SubmissionsPage : ContentPage
    {
        private readonly SubmissionsViewModel _viewModel;

        public SubmissionsPage(SubmissionsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        // Carga (o refresca) cada vez que se muestra la página.
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
