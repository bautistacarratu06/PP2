using UI.ViewModels;

namespace UI.Views
{
    public partial class TrackingPage : ContentPage
    {
        public TrackingPage()
        {
            InitializeComponent();
            BindingContext = new TrackingViewModel();
        }

        private void OnPageSizeChanged(object? sender, EventArgs e)
        {
            if (ContentHost is null || double.IsNaN(Width) || Width <= 0)
                return;

            var target = Math.Min(1240, Width);
            if (Math.Abs(ContentHost.WidthRequest - target) > 0.5)
                ContentHost.WidthRequest = target;
        }

        private void OnTableViewportChanged(object? sender, EventArgs e)
        {
            if (TableScroll is null || TableRoot is null || TableCard is null)
                return;

            if (!double.IsNaN(TableScroll.Width) && TableScroll.Width > 0)
            {
                var targetWidth = Math.Max(1120, TableScroll.Width);
                if (Math.Abs(TableRoot.WidthRequest - targetWidth) > 0.5)
                    TableRoot.WidthRequest = targetWidth;
            }

            if (double.IsNaN(TableCard.Height) || TableCard.Height <= 0)
                return;

            if (!double.IsNaN(Height) && Height > 0 && TableCard.Height > Height)
                return;

            var targetHeight = Math.Max(0, TableCard.Height - 2);
            if (targetHeight > 0 && Math.Abs(TableRoot.HeightRequest - targetHeight) > 0.5)
                TableRoot.HeightRequest = targetHeight;
        }
    }
}
