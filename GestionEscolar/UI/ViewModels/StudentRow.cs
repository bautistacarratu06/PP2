using System.Globalization;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;

namespace UI.ViewModels
{
    // Fila de grilla para la UI (Nómina y Seguimiento). Todo el formato vive acá,
    // los modelos de Core quedan como datos puros.
    public sealed class StudentRow
    {
        // Fila de Nómina: solo datos de contacto.
        public StudentRow(Student student, int rowNumber = 0)
        {
            Student = student ?? throw new ArgumentNullException(nameof(student));
            RowNumber = rowNumber;
            (BadgeBackground, BadgeForeground, BadgeGlyph) = RiskBadge.For(null);
        }

        // Fila de Seguimiento: datos + riesgo calculado.
        public StudentRow(StudentRiskResult risk, int rowNumber)
            : this(risk.Data.Student, rowNumber)
        {
            Risk = risk;
            (BadgeBackground, BadgeForeground, BadgeGlyph) = RiskBadge.For(risk.RiskLevel);
        }

        public Student Student { get; }
        public StudentRiskResult? Risk { get; }
        public int RowNumber { get; }

        public string Name => Student.Name;
        public string StudentNumber => Student.StudentNumber;
        public string Phone => Student.Phone;
        public string Email => Student.Email;
        public string LegajoLabel => $"Legajo {Student.StudentNumber}";

        public RiskLevel? Level => Risk?.RiskLevel;
        public string RiskLevel => Risk?.RiskLevel.ToString() ?? string.Empty;
        public int RiskScore => Risk?.RiskScore ?? 0;
        public int Score => Risk?.Score ?? 0;

        public string AttendanceText => Risk is null ? string.Empty : $"{Risk.Data.AttendancePercent}%";
        public string DeliveryText => Risk is null ? string.Empty : $"{Risk.Data.DeliveryPercent}%";

        public bool HasContact => Risk?.Data.LastContact is not null;

        public string ContactLabel =>
            Risk?.Data.LastContact is { } date
                ? date.ToString("dd'/'MM'/'yyyy", CultureInfo.InvariantCulture)
                : "Sin contactar";

        public string Justification
        {
            get
            {
                if (Risk is null)
                    return string.Empty;

                var d = Risk.Data;
                return $"Asistió a {d.AttendedClasses} de {d.TotalClasses} clases ({d.AttendancePercent}%) · " +
                       $"Entregó {d.SubmittedAssignments} de {d.TotalAssignments} trabajos ({d.DeliveryPercent}%) · " +
                       $"Score {Score} → Riesgo {RiskScore}";
            }
        }

        public Color BadgeBackground { get; }
        public Color BadgeForeground { get; }
        public string BadgeGlyph { get; }
    }
}
