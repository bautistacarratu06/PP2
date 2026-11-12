namespace GestionEscolar.Core.Models;

// Un alumno a insertar junto con la línea del CSV de la que salió, para poder informar errores por fila.
public sealed record NumberedStudent(int Line, Student Student);

// Fila rechazada al confirmar una importación: línea del CSV y motivo.
public sealed record RowError(int Line, string Message);

// Qué pasó al confirmar: cuántas filas se insertaron, cuántas se fusionaron y cuáles se rechazaron.
public sealed record ImportResult(int Inserted, int Updated, IReadOnlyList<RowError> Errors)
{
    public bool HasErrors => Errors.Count > 0;
}
