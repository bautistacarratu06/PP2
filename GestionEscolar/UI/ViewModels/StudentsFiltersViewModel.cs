using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace UI.ViewModels;

// Filtros por columna, orden y toggles de riesgo de la grilla de Estudiantes.
// Cualquier cambio dispara Changed; la pantalla recarga con Build.
public sealed partial class StudentsFiltersViewModel : ObservableObject
{
    private readonly HashSet<RiskLevel> _risks = [];

    public event EventHandler? Changed;

    [ObservableProperty]
    public partial string NameFilter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LegajoFilter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmailFilter { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AttendanceMinText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string AttendanceMaxText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DeliveryMinText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string DeliveryMaxText { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool FilterContactSin { get; set; }

    [ObservableProperty]
    public partial bool FilterContactCon { get; set; }

    [ObservableProperty]
    public partial string ActiveFilterKey { get; set; } = "Activos";

    [ObservableProperty]
    public partial StudentsSortField SortBy { get; set; } = StudentsSortField.Name;

    [ObservableProperty]
    public partial bool SortDescending { get; set; }

    [ObservableProperty]
    public partial string OpenFilterColumn { get; set; } = string.Empty;

    // Los checkbox del XAML son bools; todos escriben el mismo conjunto con SetRisk.
    public bool FilterRiskAlto
    {
        get => _risks.Contains(RiskLevel.Alto);
        set => SetRisk(RiskLevel.Alto, value);
    }

    public bool FilterRiskMedio
    {
        get => _risks.Contains(RiskLevel.Medio);
        set => SetRisk(RiskLevel.Medio, value);
    }

    public bool FilterRiskBajo
    {
        get => _risks.Contains(RiskLevel.Bajo);
        set => SetRisk(RiskLevel.Bajo, value);
    }

    public bool HighActive => IsOnlyRisk(RiskLevel.Alto);

    public bool MediumActive => IsOnlyRisk(RiskLevel.Medio);

    public bool LowActive => IsOnlyRisk(RiskLevel.Bajo);

    public bool HasActiveFilters => HasColumnFilters(Build(searchText: null));

    partial void OnNameFilterChanged(string value) => RaiseChanged();
    partial void OnLegajoFilterChanged(string value) => RaiseChanged();
    partial void OnEmailFilterChanged(string value) => RaiseChanged();
    partial void OnAttendanceMinTextChanged(string value) => RaiseChanged();
    partial void OnAttendanceMaxTextChanged(string value) => RaiseChanged();
    partial void OnDeliveryMinTextChanged(string value) => RaiseChanged();
    partial void OnDeliveryMaxTextChanged(string value) => RaiseChanged();
    partial void OnFilterContactSinChanged(bool value) => RaiseChanged();
    partial void OnFilterContactConChanged(bool value) => RaiseChanged();
    partial void OnActiveFilterKeyChanged(string value) => RaiseChanged();
    partial void OnSortByChanged(StudentsSortField value) => RaiseChanged();
    partial void OnSortDescendingChanged(bool value) => RaiseChanged();

    // Única vía que escribe el conjunto de niveles de riesgo filtrados.
    public void SetRisk(RiskLevel level, bool on)
    {
        var changed = on ? _risks.Add(level) : _risks.Remove(level);
        if (!changed)
            return;

        OnPropertyChanged(level switch
        {
            RiskLevel.Alto => nameof(FilterRiskAlto),
            RiskLevel.Medio => nameof(FilterRiskMedio),
            _ => nameof(FilterRiskBajo)
        });
        OnPropertyChanged(nameof(HighActive));
        OnPropertyChanged(nameof(MediumActive));
        OnPropertyChanged(nameof(LowActive));
        RaiseChanged();
    }

    [RelayCommand]
    private void ToggleRisk(string level)
    {
        if (!Enum.TryParse<RiskLevel>(level, out var target) || !Enum.IsDefined(target))
            return;

        var onlyThis = IsOnlyRisk(target);
        foreach (var candidate in Enum.GetValues<RiskLevel>())
            SetRisk(candidate, !onlyThis && candidate == target);
    }

    [RelayCommand]
    private void ToggleRiskFilter(string level)
    {
        if (!Enum.TryParse<RiskLevel>(level, out var target) || !Enum.IsDefined(target))
            return;

        SetRisk(target, !_risks.Contains(target));
    }

    [RelayCommand]
    private void ToggleFilterColumn(string column) =>
        OpenFilterColumn = OpenFilterColumn == column ? string.Empty : column;

    [RelayCommand]
    private void SortByColumn(string column)
    {
        if (!Enum.TryParse<StudentsSortField>(column, out var field))
            return;

        if (SortBy == field)
            SortDescending = !SortDescending;
        else
        {
            SortBy = field;
            SortDescending = false;
        }
    }

    [RelayCommand]
    private void Clear()
    {
        NameFilter = string.Empty;
        LegajoFilter = string.Empty;
        EmailFilter = string.Empty;
        AttendanceMinText = string.Empty;
        AttendanceMaxText = string.Empty;
        DeliveryMinText = string.Empty;
        DeliveryMaxText = string.Empty;
        foreach (var level in Enum.GetValues<RiskLevel>())
            SetRisk(level, false);
        FilterContactSin = false;
        FilterContactCon = false;
        ActiveFilterKey = "Activos";
        OpenFilterColumn = string.Empty;
        SortBy = StudentsSortField.Name;
        SortDescending = false;
    }

    public StudentsQueryFilter Build(string? searchText, int? page = null, int? pageSize = null) => new()
    {
        SearchText = searchText,
        Page = page,
        PageSize = pageSize,
        NameContains = NullIfBlank(NameFilter),
        StudentNumberContains = NullIfBlank(LegajoFilter),
        EmailContains = NullIfBlank(EmailFilter),
        RiskLevels = _risks.Count == 0 ? null : _risks.ToList(),
        AttendanceMinPercent = ParsePercent(AttendanceMinText),
        AttendanceMaxPercent = ParsePercent(AttendanceMaxText),
        DeliveryMinPercent = ParsePercent(DeliveryMinText),
        DeliveryMaxPercent = ParsePercent(DeliveryMaxText),
        ContactStatuses = BuildContactStatuses(),
        ActiveStatus = ActiveFilterKey switch
        {
            "Inactivos" => ActiveStatusFilter.InactiveOnly,
            "Todos" => ActiveStatusFilter.All,
            _ => ActiveStatusFilter.ActiveOnly
        },
        SortBy = SortBy,
        SortDescending = SortDescending
    };

    private List<ContactStatusFilter>? BuildContactStatuses()
    {
        var contacts = new List<ContactStatusFilter>();
        if (FilterContactSin) contacts.Add(ContactStatusFilter.SinContactar);
        if (FilterContactCon) contacts.Add(ContactStatusFilter.Contactado);
        return contacts.Count == 0 ? null : contacts;
    }

    private bool IsOnlyRisk(RiskLevel level) => _risks.Count == 1 && _risks.Contains(level);

    private void RaiseChanged()
    {
        OnPropertyChanged(nameof(HasActiveFilters));
        Changed?.Invoke(this, EventArgs.Empty);
    }

    private static bool HasColumnFilters(StudentsQueryFilter filter) =>
        !string.IsNullOrWhiteSpace(filter.NameContains)
        || !string.IsNullOrWhiteSpace(filter.StudentNumberContains)
        || !string.IsNullOrWhiteSpace(filter.EmailContains)
        || filter.RiskLevels is { Count: > 0 }
        || filter.AttendanceMinPercent is not null
        || filter.AttendanceMaxPercent is not null
        || filter.DeliveryMinPercent is not null
        || filter.DeliveryMaxPercent is not null
        || filter.ContactStatuses is { Count: > 0 }
        || filter.ActiveStatus != ActiveStatusFilter.ActiveOnly;

    private static string? NullIfBlank(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static int? ParsePercent(string text) =>
        int.TryParse(text.Trim(), out var value) ? Math.Clamp(value, 0, 100) : null;
}
