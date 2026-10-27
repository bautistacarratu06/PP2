using System.Globalization;
using GestionEscolar.Core.Enums;

namespace UI.Converters;

public class RiskLevelColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var soft = parameter as string == "soft";
        return Level(value) switch
        {
            RiskLevel.Alto => Color.FromArgb(soft ? "#1FF87171" : "#F87171"),
            RiskLevel.Medio => Color.FromArgb(soft ? "#1FFBBF24" : "#FBBF24"),
            RiskLevel.Bajo => Color.FromArgb(soft ? "#1F34D399" : "#34D399"),
            _ => Color.FromArgb(soft ? "#1B1B1B" : "#8E8E93")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();

    private static RiskLevel? Level(object? value) => value switch
    {
        RiskLevel level => level,
        string text when Enum.TryParse<RiskLevel>(text, out var level) => level,
        _ => null
    };
}

public class StatusKindColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var soft = parameter as string == "soft";
        return (value as string)?.ToLowerInvariant() switch
        {
            "success" => Color.FromArgb(soft ? "#1F34D399" : "#34D399"),
            "danger" => Color.FromArgb(soft ? "#1FF87171" : "#F87171"),
            "warning" => Color.FromArgb(soft ? "#1FFBBF24" : "#FBBF24"),
            _ => Color.FromArgb(soft ? "#1B1B1B" : "#8E8E93")
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
