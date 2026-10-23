namespace UI.ViewModels
{
    // Ítem de Picker con texto ya formateado para la UI (el formato no vive en Core).
    public sealed record PickerOption<T>(T Value, string Label)
    {
        // El Picker muestra ToString() cuando no hay ItemDisplayBinding (evita bindings compilados sobre genéricos).
        public override string ToString() => Label;
    }
}
