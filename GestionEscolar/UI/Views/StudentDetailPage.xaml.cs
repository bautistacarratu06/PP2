using UI.Services;
using UI.ViewModels;

namespace UI.Views
{
    public partial class StudentDetailPage : ContentPage
    {
        private readonly SectionSelection _selection;
        private readonly IMainThreadDispatcher _dispatcher;

        public StudentDetailPage(StudentDetailViewModel viewModel, SectionSelection selection, IMainThreadDispatcher? dispatcher = null)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _selection = selection;
            SizeChanged += (_, _) => Arrange();
            _dispatcher = dispatcher ?? new MauiMainThreadDispatcher();
        }

        private void Arrange()
        {
            if (double.IsNaN(Width) || Width <= 0)
                return;

            var narrow = Width < 720;
            Columns.ColumnDefinitions = narrow
                ? [new ColumnDefinition(GridLength.Star)]
                : [new ColumnDefinition(300), new ColumnDefinition(GridLength.Star)];
            Columns.RowDefinitions = narrow
                ? [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)]
                : [new RowDefinition(GridLength.Auto)];
            Columns.RowSpacing = narrow ? 16 : 0;
            Grid.SetColumn(History, narrow ? 0 : 1);
            Grid.SetRow(History, narrow ? 1 : 0);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _selection.Changed += OnSectionChanged;
        }

        protected override void OnDisappearing()
        {
            _selection.Changed -= OnSectionChanged;
            base.OnDisappearing();
        }

        private void OnSectionChanged(object? sender, EventArgs e) => _dispatcher.Post(async () =>
        {
            if (Shell.Current is not null)
                await Shell.Current.GoToAsync("//StudentsPage");
        });
    }
}
