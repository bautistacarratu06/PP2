using System.Globalization;

namespace UI.Converters
{
    // true -> false y viceversa (p. ej. deshabilitar el selector mientras IsBusy).
    public sealed class InvertBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is not true;

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
            value is not true;
    }
}
