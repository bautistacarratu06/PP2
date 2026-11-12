using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Consulta unificada de la vista Estudiantes: riesgo, filtros, orden y resumen solo sobre activos.
public class StudentsQueryService : IStudentsQueryService
{
    private readonly ITrackingRepository _repository;
    private readonly RiskConfig _config;

    public StudentsQueryService(ITrackingRepository repository, RiskConfig config)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    public async Task<StudentsQueryResult> QueryAsync(
        int sectionId,
        StudentsQueryFilter? filter = null,
        CancellationToken cancellationToken = default)
    {
        filter ??= new StudentsQueryFilter();
        var rows = await _repository.GetRowsBySectionAsync(sectionId, cancellationToken).ConfigureAwait(false);
        var evaluated = rows.Select(Evaluate).ToList();
        var activeSummary = RiskSummary.FromResults(evaluated.Where(student => student.Data.Student.IsActive));
        var sorted = ApplySort(ApplyFilters(evaluated, filter), filter).ToList();
        var page = ApplyPaging(sorted, filter).ToList();
        return new StudentsQueryResult(page, activeSummary) { TotalCount = sorted.Count };
    }

    private StudentRiskResult Evaluate(StudentTrackingData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var attended = data.TotalClasses < 0
            ? data.AttendedClasses
            : Math.Clamp(data.AttendedClasses, 0, data.TotalClasses);
        var submitted = data.TotalAssignments < 0
            ? data.SubmittedAssignments
            : Math.Clamp(data.SubmittedAssignments, 0, data.TotalAssignments);

        var evaluation = RiskRule.Evaluate(
            attended,
            data.TotalClasses,
            submitted,
            data.TotalAssignments,
            _config);
        return new StudentRiskResult(data, evaluation.Score, evaluation.Level);
    }

    private static IEnumerable<StudentRiskResult> ApplyFilters(
        IEnumerable<StudentRiskResult> students,
        StudentsQueryFilter filter)
    {
        var rows = FilterByActiveStatus(students, filter);
        rows = FilterByRisk(rows, filter);
        rows = FilterByPercent(rows, filter);
        rows = FilterByContact(rows, filter);
        rows = FilterByText(rows, filter);
        return FilterBySearch(rows, filter);
    }

    private static IEnumerable<StudentRiskResult> FilterByActiveStatus(
        IEnumerable<StudentRiskResult> rows,
        StudentsQueryFilter filter) =>
        filter.ActiveStatus switch
        {
            ActiveStatusFilter.ActiveOnly => rows.Where(s => s.Data.Student.IsActive),
            ActiveStatusFilter.InactiveOnly => rows.Where(s => !s.Data.Student.IsActive),
            _ => rows
        };

    private static IEnumerable<StudentRiskResult> FilterByRisk(
        IEnumerable<StudentRiskResult> rows,
        StudentsQueryFilter filter) =>
        filter.RiskLevels is { Count: > 0 } levels
            ? rows.Where(s => levels.Contains(s.RiskLevel))
            : rows;

    private static IEnumerable<StudentRiskResult> FilterByPercent(
        IEnumerable<StudentRiskResult> rows,
        StudentsQueryFilter filter)
    {
        if (filter.AttendanceMinPercent is int minAtt)
            rows = rows.Where(s => s.Data.AttendancePercent >= minAtt);
        if (filter.AttendanceMaxPercent is int maxAtt)
            rows = rows.Where(s => s.Data.AttendancePercent <= maxAtt);
        if (filter.DeliveryMinPercent is int minDel)
            rows = rows.Where(s => s.Data.DeliveryPercent >= minDel);
        if (filter.DeliveryMaxPercent is int maxDel)
            rows = rows.Where(s => s.Data.DeliveryPercent <= maxDel);

        return rows;
    }

    private static IEnumerable<StudentRiskResult> FilterByContact(
        IEnumerable<StudentRiskResult> rows,
        StudentsQueryFilter filter)
    {
        if (filter.ContactStatuses is not { Count: > 0 } contacts)
            return rows;

        return rows.Where(s =>
        {
            var status = s.Data.LastContact is null
                ? ContactStatusFilter.SinContactar
                : ContactStatusFilter.Contactado;
            return contacts.Contains(status);
        });
    }

    private static IEnumerable<StudentRiskResult> FilterByText(
        IEnumerable<StudentRiskResult> rows,
        StudentsQueryFilter filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.NameContains))
        {
            var needle = filter.NameContains.Trim();
            rows = rows.Where(s =>
                Contains(s.Data.Student.LastName, needle)
                || Contains(s.Data.Student.FirstName, needle)
                || Contains(s.Data.Student.Name, needle));
        }

        if (!string.IsNullOrWhiteSpace(filter.StudentNumberContains))
        {
            var needle = filter.StudentNumberContains.Trim();
            rows = rows.Where(s => Contains(s.Data.Student.StudentNumber, needle));
        }

        if (!string.IsNullOrWhiteSpace(filter.EmailContains))
        {
            var needle = filter.EmailContains.Trim();
            rows = rows.Where(s => Contains(s.Data.Student.Email, needle));
        }

        return rows;
    }

    private static IEnumerable<StudentRiskResult> FilterBySearch(
        IEnumerable<StudentRiskResult> rows,
        StudentsQueryFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SearchText))
            return rows;

        var needle = filter.SearchText.Trim();
        return rows.Where(s =>
            Contains(s.Data.Student.Name, needle)
            || Contains(s.Data.Student.LastName, needle)
            || Contains(s.Data.Student.FirstName, needle)
            || Contains(s.Data.Student.StudentNumber, needle));
    }

    // El resumen se calcula antes, sobre todos los activos; acá solo se recorta la página pedida.
    private static IEnumerable<StudentRiskResult> ApplyPaging(
        IEnumerable<StudentRiskResult> ordered,
        StudentsQueryFilter filter)
    {
        if (filter.PageSize is not int size || size <= 0)
            return ordered;

        var page = Math.Max(filter.Page ?? 1, 1);
        return ordered.Skip((page - 1) * size).Take(size);
    }

    private static IOrderedEnumerable<StudentRiskResult> ApplySort(
        IEnumerable<StudentRiskResult> students,
        StudentsQueryFilter filter)
    {
        return filter.SortBy switch
        {
            StudentsSortField.StudentNumber => Order(students, s => s.Data.Student.StudentNumber, filter.SortDescending),
            StudentsSortField.Email => Order(students, s => s.Data.Student.Email, filter.SortDescending),
            StudentsSortField.Phone => Order(students, s => s.Data.Student.Phone, filter.SortDescending),
            StudentsSortField.Risk => Order(students, s => s.RiskScore, filter.SortDescending),
            StudentsSortField.Attendance => Order(students, s => s.Data.AttendancePercent, filter.SortDescending),
            StudentsSortField.Delivery => Order(students, s => s.Data.DeliveryPercent, filter.SortDescending),
            StudentsSortField.Contact => Order(students, s => s.Data.LastContact ?? DateTime.MinValue, filter.SortDescending),
            StudentsSortField.Active => Order(students, s => s.Data.Student.IsActive, filter.SortDescending),
            _ => OrderByName(students, filter.SortDescending)
        };
    }

    private static IOrderedEnumerable<StudentRiskResult> OrderByName(
        IEnumerable<StudentRiskResult> students,
        bool descending)
    {
        if (descending)
        {
            return students
                .OrderByDescending(s => s.Data.Student.LastName, StringComparer.CurrentCulture)
                .ThenByDescending(s => s.Data.Student.FirstName, StringComparer.CurrentCulture);
        }

        return students
            .OrderBy(s => s.Data.Student.LastName, StringComparer.CurrentCulture)
            .ThenBy(s => s.Data.Student.FirstName, StringComparer.CurrentCulture);
    }

    private static IOrderedEnumerable<StudentRiskResult> Order<TKey>(
        IEnumerable<StudentRiskResult> students,
        Func<StudentRiskResult, TKey> key,
        bool descending)
    {
        var ordered = descending
            ? students.OrderByDescending(key)
            : students.OrderBy(key);
        return ordered
            .ThenBy(s => s.Data.Student.LastName, StringComparer.CurrentCulture)
            .ThenBy(s => s.Data.Student.FirstName, StringComparer.CurrentCulture);
    }

    private static bool Contains(string haystack, string needle) =>
        haystack.Contains(needle, StringComparison.OrdinalIgnoreCase);
}
