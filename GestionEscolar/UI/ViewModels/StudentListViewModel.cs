using System.Collections.ObjectModel;
using System.Linq;
using UI.Seeding;

namespace UI.ViewModels
{
    public class StudentListViewModel
    {
        public ObservableCollection<StudentRow2> FilteredStudents { get; }

        public int FilteredCount => FilteredStudents.Count;

        public StudentListViewModel()
        {
            FilteredStudents = new ObservableCollection<StudentRow2>(
                StudentSeeder.GetMockedStudentsForListGridView()
                    .Select(student => new StudentRow2
                    {
                        LegajoLabel = student.LegajoLabel,
                        Name = student.Name,
                        Phone = student.Phone,
                        Email = student.Email
                    }));
        }
    }

    public class StudentRow2
    {
        public string LegajoLabel { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
    }
}