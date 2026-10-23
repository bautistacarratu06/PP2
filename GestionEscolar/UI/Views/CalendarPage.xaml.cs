using UI.ViewModels;

namespace UI.Views
{
    public partial class CalendarPage : ContentPage
    {
        private readonly CalendarViewModel _viewModel;

        public CalendarPage(CalendarViewModel viewModel)
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
