using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;
using GestionEscolar.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GestionEscolar.Data.Repositories;

// Totales crudos. El riesgo lo calcula TrackingService.
public sealed class EfTrackingRepository(IDbContextFactory<CourseTrackingDbContext> contexts) : ITrackingRepository
{
    private readonly IDbContextFactory<CourseTrackingDbContext> _contexts = contexts ?? throw new ArgumentNullException(nameof(contexts));

    public async Task<IReadOnlyList<StudentTrackingData>> GetRowsBySectionAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var present = AttendanceStatus.Presente.ToDbValue();
        var delivered = SubmissionStatus.Entrego.ToDbValue();

        await using var db = await _contexts.CreateDbContextAsync(cancellationToken).ConfigureAwait(false);
        var rows = await db.Students
            .AsNoTracking()
            .Where(student => student.SectionId == sectionId)
            .OrderBy(student => student.LastName)
            .ThenBy(student => student.FirstName)
            .Select(student => new
            {
                student.Id,
                student.FirstName,
                student.LastName,
                student.StudentNumber,
                student.Phone,
                student.Email,
                TotalClasses = student.Section.ClassSessions.Count(),
                AttendedClasses = student.AttendanceRecords.Count(record =>
                    record.Status == present && record.ClassSession.SectionId == student.SectionId),
                TotalAssignments = student.Section.Assignments.Count(),
                SubmittedAssignments = student.Submissions.Count(submission =>
                    submission.Status == delivered && submission.Assignment.SectionId == student.SectionId),
                LastContact = student.Interventions.Max(intervention => (DateTime?)intervention.InterventionDate)
            })
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return rows.Select(row => new StudentTrackingData
        {
            Student = new Student
            {
                Id = row.Id,
                Name = row.FirstName + " " + row.LastName,
                StudentNumber = row.StudentNumber,
                Phone = row.Phone ?? string.Empty,
                Email = row.Email ?? string.Empty
            },
            TotalClasses = row.TotalClasses,
            AttendedClasses = row.AttendedClasses,
            TotalAssignments = row.TotalAssignments,
            SubmittedAssignments = row.SubmittedAssignments,
            LastContact = row.LastContact
        }).ToList();
    }
}
