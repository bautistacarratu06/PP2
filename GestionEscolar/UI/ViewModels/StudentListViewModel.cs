using System.Collections.ObjectModel;
using UI.Seeding;

namespace UI.ViewModels
{
    // TODO: Nómina todavía lee del CSV embebido. Pasarla a un repositorio como Seguimiento.
    public class StudentListViewModel
    {
        public ObservableCollection<StudentRow> FilteredStudents { get; }

        public int FilteredCount => FilteredStudents.Count;

        public StudentListViewModel()
        {
            var rowNumber = 1;
            FilteredStudents = new ObservableCollection<StudentRow>(
                StudentSeeder.GetStudents().Select(student => new StudentRow(student, rowNumber++)));
        }
    }
}
