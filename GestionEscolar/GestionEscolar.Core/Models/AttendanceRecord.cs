using GestionEscolar.Core.Enums;

namespace GestionEscolar.Core.Models;

// Una clase (class_session) de la comisión y la asistencia del alumno a esa clase.
public class AttendanceRecord
{
    // id de attendance_record; null si no hay registro para esa clase.
    public int? Id { get; set; }
    public int StudentId { get; set; }
    public int ClassSessionId { get; set; }
    public DateTime Date { get; set; }

    // null = la clase existe pero no se cargó asistencia para el alumno.
    public AttendanceStatus? Status { get; set; }

    public bool IsPresent => Status == AttendanceStatus.Presente;
}
