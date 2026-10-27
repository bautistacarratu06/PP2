using UI.ViewModels;

namespace UI.Views
{
    public partial class SettingsViewPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsViewPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        // Muestra los umbrales guardados cada vez que se abre la página.
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
