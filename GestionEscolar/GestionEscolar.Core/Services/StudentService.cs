using GestionEscolar.Core.Interfaces;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Arma el panel de seguimiento: trae los datos del repositorio de cada alumno 

public class StudentService
{
    // Crea una propiedad interna de la clase que es de tipo IStudentRepository y se llama _repository.
    private readonly IStudentRepository _repository;
    
    public StudentService(IStudentRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<IReadOnlyList<Student>> GetRosterAsync(int sectionId, CancellationToken cancellationToken = default)
    {
        var rows = await _repository.GetRosterAsync(sectionId, cancellationToken).ConfigureAwait(false);
        
        return rows.Select(row => new Student
        {
            Id = row.Id,
            Name = row.Name,
            StudentNumber = row.StudentNumber,
            Phone = row.Phone,
            Email = row.Email
        }).ToList();
    }
}   
