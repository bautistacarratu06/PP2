using UI.Models;

namespace UI.Services;

public class ScoringRiskServices
{
    // Los pesos deben sumar 1
    private const double AttendanceWeight = 0.6;
    private const double SubmissionWeight = 0.4;

    // Riesgo igual o mayor a este valor = Alto
    private const int HighThreshold = 50;

    // Riesgo igual o mayor a este valor (y menor al alto) = Medio
    private const int MediumThreshold = 25;

    private int _totalClasses;
    private int _totalAssignments;

    // Total de clases dictadas hasta la fecha
    public int TotalClasses
    {
        get => _totalClasses;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(TotalClasses), "El total no puede ser negativo.");

            _totalClasses = value;
        }
    }

    // Total de trabajos asignados hasta la fecha (a futuro vendrá de la base de datos)
    public int TotalAssignments
    {
        get => _totalAssignments;
        set
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(nameof(TotalAssignments), "El total no puede ser negativo.");

            _totalAssignments = value;
        }
    }

    public int Calculate(int attendedClasses, int submittedAssignments)
    {
        // 1. Validar los datos
        Validate(attendedClasses, TotalClasses, nameof(attendedClasses));
        Validate(submittedAssignments, TotalAssignments, nameof(submittedAssignments));

        // 2. Calcular qué porcentaje NO hizo (entre 0 y 1)
        double absenceRate = 0.0;
        if (TotalClasses > 0)
        {
            absenceRate = (double)(TotalClasses - attendedClasses) / TotalClasses;
        }

        double missingRate = 0.0;
        if (TotalAssignments > 0)
        {
            missingRate = (double)(TotalAssignments - submittedAssignments) / TotalAssignments;
        }

        // 3. Combinar con los pesos: esto ya es el riesgo (0 a 100)
        double risk = (absenceRate * AttendanceWeight + missingRate * SubmissionWeight) * 100;

        // 4. Redondear al entero más cercano y devolver
        return (int)Math.Round(risk, MidpointRounding.AwayFromZero);
    }

    public string GetRiskLevel(int risk)
    {
        if (risk >= HighThreshold)
            return "Alto";

        if (risk >= MediumThreshold)
            return "Medio";

        return "Bajo";
    }

    public void ApplyRisk(Student student)
    {
        student.RiskScore = Calculate(student.AttendedClasses, student.SubmittedAssignments);
        student.RiskLevel = GetRiskLevel(student.RiskScore);
    }

    private static void Validate(int done, int total, string paramName)
    {
        if (done < 0 || done > total)
        {
            throw new ArgumentOutOfRangeException(
                paramName,
                "Los valores no pueden ser negativos y lo realizado no puede superar al total.");
        }
    }
}