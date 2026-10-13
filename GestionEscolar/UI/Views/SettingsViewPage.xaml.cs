using UI.ViewModels;

namespace UI.Views
{
    public partial class SettingsViewPage : ContentPage
    {
        public SettingsViewPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
