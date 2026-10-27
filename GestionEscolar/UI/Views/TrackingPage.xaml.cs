using UI.ViewModels;

namespace UI.Views
{
    public partial class TrackingPage : ContentPage
    {
        private readonly TrackingViewModel _viewModel;

        public TrackingPage(TrackingViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
            SizeChanged += (_, _) => ArrangeKpis();
        }

        private void ArrangeKpis()
        {
            if (double.IsNaN(Width) || Width <= 0)
                return;

            var narrow = Width < 720;
            Kpis.ColumnDefinitions = narrow
                ? [new ColumnDefinition(GridLength.Star)]
                : [new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star)];
            Kpis.RowDefinitions = narrow
                ? [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)]
                : [new RowDefinition(GridLength.Auto)];
            Kpis.RowSpacing = narrow ? 12 : 0;
            Grid.SetColumn(MediumCard, narrow ? 0 : 1);
            Grid.SetRow(MediumCard, narrow ? 1 : 0);
            Grid.SetColumn(LowCard, narrow ? 0 : 2);
            Grid.SetRow(LowCard, narrow ? 2 : 0);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
