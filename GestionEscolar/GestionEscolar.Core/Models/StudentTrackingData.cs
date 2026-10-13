namespace GestionEscolar.Core.Models;

// Datos crudos de seguimiento de un alumno en una comisión:
// asistencia, entregas y último contacto. Lo devuelve ITrackingRepository.
public class StudentTrackingData
{
    public Student Student { get; set; } = new();
    public int TotalClasses { get; set; }
    public int AttendedClasses { get; set; }
    public int TotalAssignments { get; set; }
    public int SubmittedAssignments { get; set; }
    public DateTime? LastContact { get; set; }

    public int AttendancePercent => Percent(AttendedClasses, TotalClasses);

    public int DeliveryPercent => Percent(SubmittedAssignments, TotalAssignments);

    private static int Percent(int done, int total) =>
        total == 0 ? 0 : (int)Math.Round(done * 100.0 / total, MidpointRounding.AwayFromZero);
}
