using UI.ViewModels;

namespace UI.Services;

// Navegación entre pantallas; la ruta de Shell vive solo en la implementación.
public interface IAppNavigation
{
    Task GoToStudentsAsync();
    Task GoToStudentDetailAsync(StudentRow row, string sectionName, int sectionId);
}

public sealed class ShellAppNavigation : IAppNavigation
{
    public Task GoToStudentsAsync() => Shell.Current.GoToAsync("//StudentsPage");

    public Task GoToStudentDetailAsync(StudentRow row, string sectionName, int sectionId) =>
        Shell.Current.GoToAsync(nameof(Views.StudentDetailPage), new Dictionary<string, object>
        {
            [StudentDetailViewModel.StudentQueryKey] = row,
            [StudentDetailViewModel.SectionQueryKey] = sectionName,
            [StudentDetailViewModel.SectionIdQueryKey] = sectionId
        });
}
