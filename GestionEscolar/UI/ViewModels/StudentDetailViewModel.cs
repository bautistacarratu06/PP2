using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

namespace UI.ViewModels
{
    // Carga la nómina de alumnos desde SQL Server mediante StudentService
    public class StudentListViewModel
    {
        private readonly StudentService _studentService;

        public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

        public int FilteredCount => FilteredStudents.Count;

        // Constructor principal: .NET MAUI inyectará la instancia registrada de StudentService
        public StudentListViewModel(StudentService studentService)
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
            
            // Cargar la nómina de la comisión 1 al iniciar
            _ = LoadStudentsAsync(1);
        }

        // Método asíncrono para cargar la nómina sin bloquear la interfaz
        public async Task LoadStudentsAsync(int sectionId)
        {
            var students = await _studentService.GetRosterAsync(sectionId).ConfigureAwait(true);

            FilteredStudents.Clear();
            var rowNumber = 1;

            foreach (var student in students)
            {
                FilteredStudents.Add(new StudentRow(student, rowNumber++));
            }
        }
    }
}