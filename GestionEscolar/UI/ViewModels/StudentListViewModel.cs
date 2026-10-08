using System.Collections.ObjectModel;

namespace UI.ViewModels
{
    public class StudentListViewModel
    {
        public ObservableCollection<StudentRow2> FilteredStudents { get; }

        public int FilteredCount => FilteredStudents.Count;

        public StudentListViewModel()
        {
            FilteredStudents = new ObservableCollection<StudentRow2>
            {
                new StudentRow2
                {
                    LegajoLabel = "A1004",
                    Name = "Acosta, Elena",
                    Phone = "11 5555-1004",
                    Email = "estudiante4@ejemplo.edu.ar"
                },
                new StudentRow2
                {
                    LegajoLabel = "A1009",
                    Name = "Benítez, Martín",
                    Phone = "11 5555-1009",
                    Email = "estudiante9@ejemplo.edu.ar"
                },
                new StudentRow2
                {
                    LegajoLabel = "A1014",
                    Name = "Gómez, Lucía",
                    Phone = "11 5555-1014",
                    Email = "estudiante14@ejemplo.edu.ar"
                }
            };
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