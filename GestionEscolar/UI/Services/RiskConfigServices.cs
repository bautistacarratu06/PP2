using UI.Models;

namespace UI.Services;

public readonly record struct ThresholdValidationResult(
    bool IsValid,
    string Message
);

public static class RiskConfigService
{
    public static ThresholdValidationResult ValidateThresholds(int highThreshold, int mediumThreshold)
    {
        if (highThreshold is < 0 or > 100 || mediumThreshold is < 0 or > 100)
            return new ThresholdValidationResult(false, "Los valores deben estar entre 0 y 100.");

        if (mediumThreshold >= highThreshold)
            return new ThresholdValidationResult(false, "El umbral medio debe ser menor que el alto.");

        return new ThresholdValidationResult(true, string.Empty);
    }

    public static string GetRiskLevel(int riskScore, RiskConfig config)
    {
        if (riskScore >= config.HighThreshold)
            return "Alto";

        if (riskScore >= config.MediumThreshold)
            return "Medio";

        return "Bajo";
    }
}