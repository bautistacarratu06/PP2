using CommunityToolkit.Mvvm.ComponentModel;

namespace UI.ViewModels
{
    // Estado de ocupado, error y mensaje de estado, más el bucle que serializa las cargas:
    // si llega trabajo mientras IsBusy, se ejecuta al terminar el actual. Cada pantalla pasa
    // su propio texto de error, así no se pisan los mensajes que ya mostraba.
    public abstract partial class BusyStateViewModel : ObservableObject
    {
        private Func<Task>? _pending;

        [ObservableProperty]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        public partial string ErrorMessage { get; set; } = string.Empty;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasStatus))]
        public partial string StatusMessage { get; set; } = string.Empty;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public bool HasStatus => !string.IsNullOrEmpty(StatusMessage);

        // Hay otra carga esperando: quien está cargando puede soltar su resultado.
        protected bool HasPendingWork => _pending is not null;

        partial void OnIsBusyChanged(bool value) => OnBusyChanged();

        protected virtual void OnBusyChanged()
        {
        }

        // Se llama si el trabajo falló, después de mostrar el error.
        protected virtual void OnWorkFailed()
        {
        }

        protected async Task RunLoopAsync(Func<Task> work, string failureMessage)
        {
            if (IsBusy)
            {
                _pending = work;
                return;
            }

            IsBusy = true;
            try
            {
                var next = work;
                while (next is not null)
                {
                    _pending = null;
                    ErrorMessage = string.Empty;
                    await RunOnceAsync(next, failureMessage);
                    next = _pending;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task RunOnceAsync(Func<Task> work, string failureMessage)
        {
            try
            {
                await work();
            }
            catch (Exception ex)
            {
                ReportError(failureMessage, ex);
                OnWorkFailed();
            }
        }

        protected void ReportError(string message, Exception ex)
        {
            ErrorMessage = message;
            System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] {message} {ex}");
        }
    }
}
