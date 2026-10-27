using UI.ViewModels;

namespace UI.Views
{
    public partial class StudentListPage : ContentPage
    {
        public StudentListPage(StudentListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _viewModel = viewModel;
        }

        private readonly StudentListViewModel _viewModel;

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
