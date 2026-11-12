namespace UI.Services;

// Fecha de hoy para los ViewModels, así se puede reemplazar en pruebas.
public interface IClock
{
    DateTime Today { get; }
}

public sealed class SystemClock : IClock
{
    public DateTime Today => DateTime.Today;
}
