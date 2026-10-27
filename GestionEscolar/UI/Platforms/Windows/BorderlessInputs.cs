using Microsoft.Maui.Handlers;

namespace UI;

/// <summary>
/// Quita el chrome nativo de WinUI (borde/padding) para que FieldBorder / SearchBox / SectionPicker
/// sean el único marco visible.
/// </summary>
internal static class BorderlessInputs
{
    public static void Configure()
    {
        EntryHandler.Mapper.AppendToMapping("GestionEscolarBorderless", static (handler, _) =>
        {
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0);
        });

        EditorHandler.Mapper.AppendToMapping("GestionEscolarBorderless", static (handler, _) =>
        {
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0);
        });

        PickerHandler.Mapper.AppendToMapping("GestionEscolarBorderless", static (handler, _) =>
        {
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(0);
        });

        DatePickerHandler.Mapper.AppendToMapping("GestionEscolarBorderless", static (handler, _) =>
        {
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.CornerRadius = new Microsoft.UI.Xaml.CornerRadius(0);
        });

        TimePickerHandler.Mapper.AppendToMapping("GestionEscolarBorderless", static (handler, _) =>
        {
            handler.PlatformView.BorderThickness = new Microsoft.UI.Xaml.Thickness(0);
            handler.PlatformView.Padding = new Microsoft.UI.Xaml.Thickness(0);
        });
    }
}