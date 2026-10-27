using UI.ViewModels;

namespace UI.Views
{
    public partial class StudentListPage : ContentPage
    {
        private readonly StudentListViewModel _viewModel;
        private readonly HashSet<object> _wiredDrops = [];

        public StudentListPage(StudentListViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            Wizard.BindingContext = viewModel.Import;
            Wizard.HandlerChanged += (_, _) => WireFileDrop(Wizard.Handler?.PlatformView);
            _viewModel = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            WireFileDrop(Handler?.PlatformView);
            WireFileDrop(Wizard.Handler?.PlatformView);
            await _viewModel.LoadCommand.ExecuteAsync(null);
        }

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            WireFileDrop(Handler?.PlatformView);
        }

        private void OnNewCommission(object? sender, EventArgs e) => _viewModel.Import.CreateNew = true;

        private void OnExistingCommission(object? sender, EventArgs e) => _viewModel.Import.CreateNew = false;

        private void WireFileDrop(object? platformView)
        {
#if WINDOWS
            if (platformView is not Microsoft.UI.Xaml.UIElement element || !_wiredDrops.Add(element))
                return;

            element.AllowDrop = true;
            element.DragOver += (_, args) =>
            {
                if (!_viewModel.Import.IsOpen)
                    return;

                args.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
                args.Handled = true;
            };
            element.Drop += async (_, args) =>
            {
                if (!_viewModel.Import.IsOpen)
                    return;

                if (!args.DataView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.StorageItems))
                    return;

                var items = await args.DataView.GetStorageItemsAsync();
                if (items.FirstOrDefault() is not Windows.Storage.StorageFile file)
                    return;

                await MainThread.InvokeOnMainThreadAsync(() => _viewModel.Import.AcceptFileAsync(file.Path));
            };
#endif
        }
    }
}
