using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;

namespace UI.ViewModels
{
    // Recibe la fila de Seguimiento ("Student", un StudentRow) y la comisión elegida ("SectionId"/"SectionName").
    // El nivel y el score de riesgo vienen de la fila (iguales a la grilla); contacto, comisión e
    // historiales se leen de la base con StudentDetailsService.
    public partial class StudentDetailViewModel : ObservableObject, IQueryAttributable
    {
        public const string StudentQueryKey = "Student";
        public const string SectionQueryKey = "SectionName";
        public const string SectionIdQueryKey = "SectionId";

        private const string DateFormat = "dd'/'MM'/'yyyy";

        private readonly StudentDetailsService _detailsService;

        public StudentDetailViewModel(StudentDetailsService detailsService)
        {
            _detailsService = detailsService;
        }

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string StudentNumber { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Commission { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Phone { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string Email { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string RiskLevel { get; set; } = string.Empty;

        [ObservableProperty]
        public partial int RiskScore { get; set; }

        [ObservableProperty]
        public partial string Justification { get; set; } = string.Empty;

        [ObservableProperty]
        public partial Color BadgeBackground { get; set; } = RiskBadge.For(null).Background;

        [ObservableProperty]
        public partial Color BadgeForeground { get; set; } = RiskBadge.For(null).Foreground;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasNoAttendance), nameof(HasNoSubmissions), nameof(HasNoInterventions))]
        public partial bool IsBusy { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasError))]
        public partial string ErrorMessage { get; set; } = string.Empty;

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public ObservableCollection<HistoryItem> Attendance { get; } = new();

        public ObservableCollection<HistoryItem> Submissions { get; } = new();

        public ObservableCollection<HistoryItem> Interventions { get; } = new();

        // Estados vacíos: solo cuando ya terminó de cargar.
        public bool HasNoAttendance => !IsBusy && Attendance.Count == 0;

        public bool HasNoSubmissions => !IsBusy && Submissions.Count == 0;

        public bool HasNoInterventions => !IsBusy && Interventions.Count == 0;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.TryGetValue(SectionQueryKey, out var section) && section is string sectionName && sectionName.Length > 0)
                Commission = sectionName;

            if (!query.TryGetValue(StudentQueryKey, out var value) || value is not StudentRow row)
                return;

            // Datos de la fila: se muestran enseguida y el riesgo queda igual que en la grilla.
            Name = row.Name;
            StudentNumber = row.StudentNumber;
            Phone = row.Phone;
            Email = row.Email;
            RiskLevel = row.RiskLevel;
            RiskScore = row.RiskScore;
            Justification = row.Justification;
            BadgeBackground = row.BadgeBackground;
            BadgeForeground = row.BadgeForeground;

            int? sectionId = query.TryGetValue(SectionIdQueryKey, out var id) && id is int sid && sid > 0 ? sid : null;
            _ = LoadAsync(row.Student.Id, sectionId);
        }

        private async Task LoadAsync(int studentId, int? sectionId)
        {
            IsBusy = true;
            ErrorMessage = string.Empty;
            Attendance.Clear();
            Submissions.Clear();
            Interventions.Clear();
            try
            {
                var detail = await _detailsService.GetAsync(studentId, sectionId);
                if (detail is null)
                {
                    ErrorMessage = "No se encontró el alumno.";
                    return;
                }

                Phone = detail.PhoneNumber;
                Email = detail.Email;
                StudentNumber = detail.StudentNumber;
                if (string.IsNullOrEmpty(Commission) || sectionId is null || sectionId == detail.SectionId)
                    Commission = detail.SectionName;

                foreach (var a in detail.AttendanceRecords)
                    Attendance.Add(new HistoryItem($"Clase · {Format(a.Date)}", AttendanceText(a.Status)));

                foreach (var s in detail.Submissions)
                    Submissions.Add(new HistoryItem($"{s.AssignmentName} · vence {Format(s.DueDate)}", SubmissionText(s.Status)));

                foreach (var i in detail.Interventions)
                    Interventions.Add(new HistoryItem($"{Format(i.Date)} · {i.Channel}", i.Description));
            }
            catch (Exception ex)
            {
                ErrorMessage = "No se pudo cargar el detalle del alumno.";
                System.Diagnostics.Debug.WriteLine($"No se pudo cargar el detalle del alumno {studentId}: {ex}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static string Format(DateTime date) => date.ToString(DateFormat, CultureInfo.InvariantCulture);

        private static string AttendanceText(AttendanceStatus? status) => status switch
        {
            AttendanceStatus.Presente => "Presente",
            AttendanceStatus.Ausente => "Ausente",
            _ => "Sin registro"
        };

        private static string SubmissionText(SubmissionStatus? status) => status switch
        {
            SubmissionStatus.Entrego => "Entregó",
            SubmissionStatus.NoEntrego => "No entregó",
            _ => "Sin registro"
        };
    }

    public sealed record HistoryItem(string Label, string Status);
}
