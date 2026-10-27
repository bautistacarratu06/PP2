using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Arma la ficha completa del alumno (contacto + historiales) a partir del repositorio.
public class StudentDetailsService
{
    private readonly IStudentDetailsRepository _repository;

    public StudentDetailsService(IStudentDetailsRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    // sectionId: comisión elegida en Seguimiento. Si es null se usa la del alumno.
    public async Task<StudentDetail?> GetAsync(int studentId, int? sectionId = null, CancellationToken cancellationToken = default)
    {
        var detail = await _repository.GetStudentAsync(studentId, cancellationToken).ConfigureAwait(false);
        if (detail is null)
            return null;

        var section = sectionId ?? detail.SectionId;

        var attendanceTask = _repository.GetAttendanceRecordsAsync(studentId, section, cancellationToken);
        var submissionsTask = _repository.GetSubmissionsAsync(studentId, section, cancellationToken);
        var interventionsTask = _repository.GetInterventionsAsync(studentId, cancellationToken);
        await Task.WhenAll(attendanceTask, submissionsTask, interventionsTask).ConfigureAwait(false);

        detail.AttendanceRecords = attendanceTask.Result;
        detail.Submissions = submissionsTask.Result;
        detail.Interventions = interventionsTask.Result;
        return detail;
    }

    public async Task<Intervention> RegisterContactAsync(
        int studentId,
        DateTime date,
        ContactChannel? channel,
        ContactOutcome? outcome,
        string? note,
        DateTime today,
        CancellationToken cancellationToken = default)
    {
        if (channel is null)
            throw new ContactValidationException("El medio no puede estar vacío.");

        if (outcome is null)
            throw new ContactValidationException("El resultado no puede estar vacío.");

        var day = date.Date;
        if (day > today.Date)
            throw new ContactValidationException("La fecha no puede ser posterior a hoy.");

        var trimmed = note?.Trim() ?? string.Empty;
        if (trimmed.Length > StudentEntryRules.MaxNoteLength)
            throw new ContactValidationException($"La nota no puede superar {StudentEntryRules.MaxNoteLength} caracteres.");

        var intervention = new Intervention
        {
            StudentId = studentId,
            Date = day,
            Channel = channel.Value.ToDbValue(),
            Outcome = outcome.Value.ToDbValue(),
            Note = trimmed.Length == 0 ? null : trimmed
        };

        return await _repository.AddInterventionAsync(intervention, cancellationToken).ConfigureAwait(false);
    }
}
