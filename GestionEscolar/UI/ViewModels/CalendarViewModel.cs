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

        // Permite asignar nombre tanto a trabajos como a clases personalizadas.
        public string NamePlaceholder => IsAssignment ? "Nombre del trabajo" : "Nombre de la clase (opcional)";

        public ObservableCollection<CalendarRow> Items { get; } = new();

        [ObservableProperty]
        public partial bool HasNoItems { get; set; }

        partial void OnSelectedTypeChanged(string value)
        {
            // Mantenemos el texto ingresado para no borrarlo al alternar el tipo.
        }

        protected override async Task OnSectionChangedAsync()
        {
            Items.Clear();
            HasNoItems = false;
            if (SelectedSection is null)
                return;

            var items = await _service.GetItemsAsync(SelectedSection.Id);
            foreach (var i in items)
            {
                var displayName = !string.IsNullOrWhiteSpace(i.Name)
                    ? i.Name
                    : (i.Kind == CalendarItemKind.Clase ? ClassType : AssignmentType);

                Items.Add(new CalendarRow(
                    i.Id,
                    i.Kind,
                    displayName,
                    i.Kind == CalendarItemKind.Clase ? ClassType : AssignmentType,
                    FormatDate(i.Date)));
            }

            HasNoItems = Items.Count == 0;
        }

        public IAsyncRelayCommand<CalendarRow?> EditCommand => EditClassCommand;

        [RelayCommand]
        private async Task EditClassAsync(CalendarRow? item)
        {
            if (item is null)
                return;

            var page = Application.Current?.Windows.FirstOrDefault()?.Page ?? Application.Current?.MainPage;
            if (page is null)
                return;

            var newName = await page.DisplayPromptAsync(
                "Renombrar",
                "Ingresá el nuevo nombre:",
                initialValue: item.Title);

            if (newName is null)
                return;

            var trimmed = newName.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
                return;

            item.Name = trimmed;

            try
            {
                await _service.UpdateItemNameAsync(item.Id, item.ItemKind, trimmed);
            }
            catch (Exception ex)
            {
                ReportError("No se pudo actualizar el nombre.", ex);
            }
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
            var trimmed = NewName?.Trim();
            var hasCustomName = !string.IsNullOrWhiteSpace(trimmed);

            if (!IsAssignment)
            {
                var customName = hasCustomName ? trimmed : null;
                await _service.AddClassAsync(sectionId, NewDate, customName);

                return hasCustomName
                    ? $"Clase agregada: {trimmed} · {FormatDate(NewDate)}"
                    : $"Clase agregada: {FormatDate(NewDate)}";
            }

            await _service.AddAssignmentAsync(sectionId, trimmed, NewDate);
            return $"Trabajo agregado: {trimmed} · {FormatDate(NewDate)}";
        }
    }

    public partial class CalendarRow : ObservableObject
    {
        public int Id { get; init; }

        public CalendarItemKind ItemKind { get; init; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(Title))]
        public partial string Name { get; set; } = string.Empty;

        public string Title => Name;

        [ObservableProperty]
        public partial string Kind { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string DateText { get; set; } = string.Empty;

        public string KindKey => Kind == "Trabajo" ? "accent" : "neutral";

        public CalendarRow(int id, CalendarItemKind itemKind, string name, string kind, string dateText)
        {
            Id = id;
            ItemKind = itemKind;
            Name = name;
            Kind = kind;
            DateText = dateText;
        }

        public CalendarRow(string name, string kind, string dateText)
            : this(0, kind == "Trabajo" ? CalendarItemKind.Trabajo : CalendarItemKind.Clase, name, kind, dateText)
        {
        }
    }
}
