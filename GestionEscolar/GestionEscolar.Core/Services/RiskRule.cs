using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace GestionEscolar.Core.Services;

// Regla pura de riesgo: asistencias, entregas, totales y umbrales entran juntos
// y devuelven puntaje y nivel. 60 % inasistencia, 40 % entregas faltantes,
// redondeo alejándose de cero. No guarda totales entre llamadas.
public static class RiskRule
{
    private const double AbsenceWeight = 0.6;
    private const double MissingDeliveryWeight = 0.4;

    public readonly record struct Evaluation(int Score, RiskLevel Level);

    public static Evaluation Evaluate(
        int attendedClasses,
        int totalClasses,
        int submittedAssignments,
        int totalAssignments,
        RiskConfig thresholds)
    {
        ArgumentNullException.ThrowIfNull(thresholds);

        if (totalClasses < 0)
            throw new ArgumentOutOfRangeException(nameof(totalClasses), "El total no puede ser negativo.");

        if (totalAssignments < 0)
            throw new ArgumentOutOfRangeException(nameof(totalAssignments), "El total no puede ser negativo.");

        Validate(attendedClasses, totalClasses, nameof(attendedClasses));
        Validate(submittedAssignments, totalAssignments, nameof(submittedAssignments));

        var score = Score(attendedClasses, totalClasses, submittedAssignments, totalAssignments);
        return new Evaluation(score, Classify(score, thresholds));
    }

    public static RiskLevel Classify(int score, RiskConfig thresholds)
    {
        ArgumentNullException.ThrowIfNull(thresholds);

        if (score >= thresholds.HighThreshold)
            return RiskLevel.Alto;

        if (score >= thresholds.MediumThreshold)
            return RiskLevel.Medio;

        return RiskLevel.Bajo;
    }

    private static int Score(int attended, int totalClasses, int submitted, int totalAssignments)
    {
        var absenceRate = totalClasses > 0
            ? (double)(totalClasses - attended) / totalClasses
            : 0.0;

        var missingRate = totalAssignments > 0
            ? (double)(totalAssignments - submitted) / totalAssignments
            : 0.0;

        var risk = (absenceRate * AbsenceWeight + missingRate * MissingDeliveryWeight) * 100;
        return (int)Math.Round(risk, MidpointRounding.AwayFromZero);
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
