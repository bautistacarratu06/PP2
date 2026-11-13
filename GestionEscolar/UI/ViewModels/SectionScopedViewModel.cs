using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Models;
using UI.Services;

namespace UI.ViewModels
{
    // Base para las pantallas que trabajan sobre una comisión. La comisión elegida es la
    // de SectionSelection (una sola en la ventana). El estado de ocupado y las cargas
    // serializadas vienen de BusyStateViewModel. La página llama Subscribe/Unsubscribe
    // desde OnAppearing/OnDisappearing para seguir los cambios de comisión.
    public abstract partial class SectionScopedViewModel : BusyStateViewModel
    {
        private const string LoadFailedMessage = "No se pudieron cargar los datos.";

        private readonly SectionSelection _selection;
        private readonly SectionChangeSubscription _subscription;
        private bool _suppressSectionReload;

        protected SectionScopedViewModel(SectionSelection selection, IMainThreadDispatcher? dispatcher = null)
        {
            _selection = selection;
            _subscription = new SectionChangeSubscription(selection, SyncSelectedSection, dispatcher);
        }

        public ObservableCollection<Section> Sections { get; } = new();

        [ObservableProperty]
        public partial Section? SelectedSection { get; set; }

        public void Subscribe() => _subscription.Subscribe();

        public void Unsubscribe() => _subscription.Unsubscribe();

        // Se llama desde OnAppearing de la página.
        [RelayCommand]
        private async Task LoadAsync()
        {
            if (Sections.Count == 0)
            {
                try
                {
                    await _selection.EnsureLoadedAsync();
                    foreach (var section in _selection.Items)
                        Sections.Add(section);

                    _suppressSectionReload = true;
                    SelectedSection = _selection.Current;
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

            _selection.Select(value);
            _ = RunAsync(OnSectionChangedAsync);
        }

        private void SyncSelectedSection()
        {
            var current = _selection.Current;
            if (current is null || SelectedSection?.Id == current.Id)
                return;

            SelectedSection = current;
        }

        // Recarga todo lo que depende de la comisión seleccionada.
        protected abstract Task OnSectionChangedAsync();

        protected Task RunAsync(Func<Task> work) => RunLoopAsync(work, LoadFailedMessage);

        protected static string FormatDate(DateTime date) =>
            date.ToString("dd'/'MM'/'yyyy", System.Globalization.CultureInfo.InvariantCulture);
    }
}
