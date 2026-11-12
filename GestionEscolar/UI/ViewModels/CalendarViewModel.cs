using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels
{
    // Calendario: alta de clases/trabajos y lista combinada por fecha de la comisión.
    public partial class CalendarViewModel : SectionScopedViewModel
    {
        private const string ClassType = "Clase";
        private const string AssignmentType = "Trabajo";

        private readonly ICalendarService _service;
        private readonly IClock _clock;

        public CalendarViewModel(SectionSelection selection, ICalendarService service, IClock clock) : base(selection)
        {
            _service = service;
            _clock = clock;
            SelectedType = ClassType;
            NewDate = clock.Today;
        }

        public IReadOnlyList<string> TypeOptions { get; } = [ClassType, AssignmentType];

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsAssignment), nameof(NamePlaceholder))]
        public partial string SelectedType { get; set; }

        [ObservableProperty]
        public partial string NewName { get; set; } = string.Empty;

        [ObservableProperty]
        public partial DateTime NewDate { get; set; }

        public bool IsAssignment => SelectedType == AssignmentType;

        // La clase no tiene nombre: la identifica la fecha.
        public string NamePlaceholder => IsAssignment ? "Nombre del trabajo" : "La fecha identifica la clase";

        public ObservableCollection<CalendarRow> Items { get; } = new();

        [ObservableProperty]
        public partial bool HasNoItems { get; set; }

        partial void OnSelectedTypeChanged(string value)
        {
            if (value != AssignmentType)
                NewName = string.Empty;
        }

        protected override async Task OnSectionChangedAsync()
        {
            Items.Clear();
            HasNoItems = false;
            if (SelectedSection is null)
                return;

            var items = await _service.GetItemsAsync(SelectedSection.Id);
            foreach (var i in items)
                Items.Add(new CalendarRow(
                    i.Kind == CalendarItemKind.Clase ? ClassType : i.Name ?? string.Empty,
                    i.Kind == CalendarItemKind.Clase ? ClassType : AssignmentType,
                    FormatDate(i.Date)));
            HasNoItems = Items.Count == 0;
        }

        [RelayCommand]
        private async Task AddAsync()
        {
            if (SelectedSection is null || IsBusy)
                return;

            ErrorMessage = string.Empty;
            StatusMessage = string.Empty;
            try
            {
                StatusMessage = await AddEntryAsync(SelectedSection.Id);
                NewName = string.Empty;
                await RunAsync(OnSectionChangedAsync);
            }
            catch (CalendarValidationException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception ex)
            {
                ReportError("No se pudo agregar.", ex);
            }
        }

        // Guarda la clase o el trabajo y devuelve el mensaje de éxito.
        private async Task<string> AddEntryAsync(int sectionId)
        {
            if (!IsAssignment)
            {
                await _service.AddClassAsync(sectionId, NewDate);
                return $"Clase agregada: {FormatDate(NewDate)}";
            }

            await _service.AddAssignmentAsync(sectionId, NewName, NewDate);
            return $"Trabajo agregado: {NewName.Trim()} · {FormatDate(NewDate)}";
        }
    }

    public sealed record CalendarRow(string Name, string Kind, string DateText)
    {
        public string KindKey => Kind == "Trabajo" ? "accent" : "neutral";
    }
}
