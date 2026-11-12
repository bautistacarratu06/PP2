using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Contrato del caso de uso StudentDetailsService para inyectarlo en la UI.
public interface IStudentDetailsService
{
    Task<StudentDetail?> GetAsync(int studentId, int? sectionId = null, CancellationToken cancellationToken = default);
    Task<Intervention> RegisterContactAsync(int studentId, DateTime date, ContactChannel? channel, ContactOutcome? outcome, string? note, DateTime today, CancellationToken cancellationToken = default);
}
