using UI.ViewModels;

namespace UI.Views
{
    public partial class ReportsPage : ContentPage
    {
        private readonly ReportsViewModel _viewModel;

        public ReportsPage(ReportsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.Subscribe();
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }

        protected override void OnDisappearing()
        {
            _viewModel.Unsubscribe();
            base.OnDisappearing();
        }
    }
}
