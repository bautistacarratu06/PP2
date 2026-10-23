using UI.ViewModels;

namespace UI.Views
{
    public partial class AttendancePage : ContentPage
    {
        private readonly AttendanceViewModel _viewModel;

        public AttendancePage(AttendanceViewModel viewModel)
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
