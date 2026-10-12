namespace UI.Services;

public readonly record struct StudentScoringResult(
    int Score,
    string Justification
);

public static class StudentScoringService
{
    public static StudentScoringResult Calculate(
        int attendedClasses,
        int totalClasses,
        int submittedAssignments,
        int totalAssignments,
        int riskScore)
    {
        int attendancePercent = totalClasses == 0
            ? 0
            : (int)Math.Round(
                attendedClasses * 100.0 / totalClasses,
                MidpointRounding.AwayFromZero);

        int deliveryPercent = totalAssignments == 0
            ? 0
            : (int)Math.Round(
                submittedAssignments * 100.0 / totalAssignments,
                MidpointRounding.AwayFromZero);

        int score = Math.Clamp(100 - riskScore, 0, 100);

        string justification =
            $"Asistió a {attendedClasses} de {totalClasses} clases " +
            $"({attendancePercent}%) · " +
            $"Entregó {submittedAssignments} de {totalAssignments} trabajos " +
            $"({deliveryPercent}%) · " +
            $"Score {score} → Riesgo {riskScore}";

        return new StudentScoringResult(score, justification);
    }
}