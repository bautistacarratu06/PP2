using UI.Models;

namespace UI.Seeding;

// TODO: Agustín Vera podría hacer algo parecido para crear CSVs. foreach (var student in StudentSeeder.GetMockedStudents()) 
// y crear las rows del CSV. ALgo a tener en cuenta es que este Student es mas un parcial de Student (fabricación pura) con solo campos de UI
// El tipo Student debería ser más parecido a StudentRow para que se pueda usar en la UI.
// En otro PR podemos corregirlo.
public static class StudentSeeder
{
    public static IReadOnlyList<Student> GetMockedStudents()
    {
        return
        [
            Create("Gómez, Ana", "A1020", "Alto", 100, 0, 0, null),
            Create("Acosta, Elena", "A1004", "Alto", 60, 0, 4, null),
            Create("Acosta, Elena", "A1024", "Alto", 60, 0, 4, null),
            Create("Pérez, Bruno", "A1016", "Alto", 60, 0, 4, null),
            Create("Rossi, Diego", "A1008", "Alto", 60, 0, 4, null),
            Create("Rossi, Diego", "A1028", "Alto", 60, 0, 4, null),
            Create("Sosa, Camila", "A1012", "Alto", 60, 0, 4, null),
            Create("Gómez, Ana", "A1005", "Medio", 40, 8, 0, null),
            Create("Gómez, Ana", "A1010", "Medio", 40, 8, 0, null),
            Create("Torres, Valentina", "A1033", "Medio", 40, 8, 0, null),
            Create("Ruiz, Joaquín", "A1035", "Medio", 40, 8, 0, new DateTime(2026, 9, 15)),
            Create("Molina, Sofía", "A1037", "Medio", 40, 8, 0, null),
            Create("Navarro, Lucía", "A1041", "Bajo", 10, 8, 4, new DateTime(2026, 9, 18)),
            Create("Castro, Mateo", "A1043", "Bajo", 12, 8, 3, null),
            Create("Romero, Juana", "A1045", "Bajo", 15, 7, 4, new DateTime(2026, 9, 22)),
            Create("Vargas, Nicolás", "A1047", "Bajo", 10, 8, 4, null),
            Create("Herrera, Camila", "A1049", "Bajo", 18, 8, 3, new DateTime(2026, 9, 5)),
            Create("Medina, Thiago", "A1051", "Bajo", 14, 7, 4, null),
            Create("Ortiz, Valentina", "A1053", "Bajo", 12, 8, 4, new DateTime(2026, 8, 28)),
            Create("Silva, Benjamín", "A1055", "Bajo", 16, 8, 3, null),
            Create("Ibáñez, Martina", "A1057", "Bajo", 20, 7, 4, new DateTime(2026, 9, 15)),
            Create("Cabrera, Santiago", "A1059", "Bajo", 10, 8, 4, null),
            Create("Rojas, Emilia", "A1061", "Bajo", 15, 8, 3, null),
            Create("Paredes, Facundo", "A1063", "Bajo", 18, 7, 4, new DateTime(2026, 9, 1)),
            Create("Aguirre, Renata", "A1065", "Bajo", 11, 8, 4, null),
            Create("Figueroa, Lautaro", "A1067", "Bajo", 14, 8, 3, new DateTime(2026, 9, 20)),
            Create("Benítez, Catalina", "A1069", "Bajo", 17, 7, 4, null),
            Create("Morales, Ignacio", "A1071", "Bajo", 10, 8, 4, null),
            Create("Delgado, Paula", "A1073", "Bajo", 19, 8, 3, new DateTime(2026, 8, 30)),
            Create("Vega, Tomás", "A1075", "Bajo", 13, 7, 4, null)
        ];
    }

    private static Student Create(
        string name,
        string studentNumber,
        string riskLevel,
        int riskScore,
        int attendedClasses,
        int submittedAssignments,
        DateTime? lastContact) =>
        new()
        {
            Name = name,
            StudentNumber = studentNumber,
            RiskLevel = riskLevel,
            RiskScore = riskScore,
            AttendedClasses = attendedClasses,
            TotalClasses = 8,
            SubmittedAssignments = submittedAssignments,
            TotalAssignments = 4,
            LastContact = lastContact
        };
}
