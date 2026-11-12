namespace GestionEscolar.Core.Models;

// Cómo se muestra un alumno en los mensajes: "Apellido, Nombre".
public static class StudentNameFormatter
{
    public static string Label(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        if (string.IsNullOrWhiteSpace(student.LastName) && string.IsNullOrWhiteSpace(student.FirstName))
            return student.Name;

        return $"{student.LastName}, {student.FirstName}";
    }
}
