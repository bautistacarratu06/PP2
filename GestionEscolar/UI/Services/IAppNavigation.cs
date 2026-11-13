namespace UI.Services;

// Navegación entre pantallas; la ruta de Shell vive solo en la implementación.
public interface IAppNavigation
{
    Task GoToStudentsAsync();
}

public sealed class ShellAppNavigation : IAppNavigation
{
    public Task GoToStudentsAsync() => Shell.Current.GoToAsync("//StudentsPage");
}
