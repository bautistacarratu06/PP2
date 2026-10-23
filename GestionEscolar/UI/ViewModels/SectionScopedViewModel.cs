using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

namespace UI.ViewModels
{
    // Base para las pantallas que trabajan sobre una comisión (selector igual al de Seguimiento).
    // Carga las comisiones una vez, elige la de por defecto sin recargar dos veces y serializa
    // las cargas: si llega un cambio mientras IsBusy, se ejecuta al terminar la carga actual.
    public abstract partial class SectionScopedViewModel : ObservableObject
    {
        private readonly SectionService _sectionService;
        private bool _suppressSectionReload;
        private Func<Task>? _pending;

        protected SectionScopedViewModel(SectionService sectionService)
        {
            _sectionService = sectionService;
        }

        public ObservableCollection<Section> Sections { get; } = new();

        [ObservableProperty]
        public partial Section? SelectedSection { get; set; }

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

        // Se llama desde OnAppearing de la página.
        [RelayCommand]
        private async Task LoadAsync()
        {
            if (Sections.Count == 0)
            {
                try
                {
                    var sections = await _sectionService.GetAllAsync();
                    foreach (var section in sections)
                        Sections.Add(section);

                    _suppressSectionReload = true;
                    SelectedSection = SectionService.PickDefault(sections);
                    _suppressSectionReload = false;
                }
                catch (Exception ex)
                {
                    ReportError("No se pudieron cargar las comisiones.", ex);
                    return;
                }
            }

            await RunAsync(OnSectionChangedAsync);
        }

        partial void OnSelectedSectionChanged(Section? value)
        {
            if (_suppressSectionReload || value is null)
                return;

            _ = RunAsync(OnSectionChangedAsync);
        }

        // Recarga todo lo que depende de la comisión seleccionada.
        protected abstract Task OnSectionChangedAsync();

        protected async Task RunAsync(Func<Task> work)
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
                    try
                    {
                        await next();
                    }
                    catch (Exception ex)
                    {
                        ReportError("No se pudieron cargar los datos.", ex);
                    }
                    next = _pending;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected void ReportError(string message, Exception ex)
        {
            ErrorMessage = message;
            System.Diagnostics.Debug.WriteLine($"[{GetType().Name}] {message} {ex}");
        }

        protected static string FormatDate(DateTime date) =>
            date.ToString("dd'/'MM'/'yyyy", System.Globalization.CultureInfo.InvariantCulture);
    }
}
