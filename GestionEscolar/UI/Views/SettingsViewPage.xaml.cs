using UI.ViewModels;

namespace UI.Views
{
    public partial class SettingsViewPage : ContentPage
    {
        public SettingsViewPage()
        {
            InitializeComponent();
            BindingContext = new SettingsViewModel();
        }
    }
}