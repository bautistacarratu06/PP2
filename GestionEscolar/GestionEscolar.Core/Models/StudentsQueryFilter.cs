using GestionEscolar.Core.Enums;

namespace GestionEscolar.Core.Models;

// Criterios de la consulta unificada de estudiantes. Los filtros se combinan con AND.
public sealed class StudentsQueryFilter
{
    public string? SearchText { get; init; }

    public string? NameContains { get; init; }

    public string? StudentNumberContains { get; init; }

    public string? EmailContains { get; init; }

    public IReadOnlyList<RiskLevel>? RiskLevels { get; init; }

    public int? AttendanceMinPercent { get; init; }

    public int? AttendanceMaxPercent { get; init; }

    public int? DeliveryMinPercent { get; init; }

    public int? DeliveryMaxPercent { get; init; }

    public IReadOnlyList<ContactStatusFilter>? ContactStatuses { get; init; }

    public ActiveStatusFilter ActiveStatus { get; init; } = ActiveStatusFilter.ActiveOnly;

    public StudentsSortField SortBy { get; init; } = StudentsSortField.Name;

    public bool SortDescending { get; init; }
}
