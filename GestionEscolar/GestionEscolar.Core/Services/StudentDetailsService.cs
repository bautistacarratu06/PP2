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
}
