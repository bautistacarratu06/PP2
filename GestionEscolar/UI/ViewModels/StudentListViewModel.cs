using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Data;
using GestionEscolar.Data.Repositories;

namespace UI.ViewModels
{
    // Carga la nómina de alumnos desde SQL Server mediante StudentService y SqlStudentRepository
    public class StudentListViewModel
    {
        private readonly StudentService _studentService;

        public ObservableCollection<StudentRow> FilteredStudents { get; } = new();

        public int FilteredCount => FilteredStudents.Count;

        // Constructor principal que utiliza Inyección de Dependencias (recomendado para .NET MAUI)
        public StudentListViewModel(StudentService studentService)
        {
            _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
        }

        // Constructor sin parámetros para fallback o compatibilidad directa
        public StudentListViewModel()
        {
            // Instanciación correcta usando el Singleton DatabaseInstance.GetInstance()
            var db = DatabaseInstance.GetInstance("MainConnection");
            var repository = new SqlStudentRepository(db);
            _studentService = new StudentService(repository);

            // Cargar alumnos de la comisión 1
            _ = LoadStudentsAsync(1);
        }

        // Método asíncrono para cargar la nómina sin congelar la interfaz
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