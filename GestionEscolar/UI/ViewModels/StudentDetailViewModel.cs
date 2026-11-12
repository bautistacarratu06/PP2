using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Formatting;
using UI.Services;

namespace UI.ViewModels
{
    // Recibe la fila de Seguimiento ("Student", un StudentRow) y la comisión elegida ("SectionId"/"SectionName").
    // El nivel y el score de riesgo vienen de la fila (iguales a la grilla); contacto, comisión e
    // historiales se leen de la base con IStudentDetailsService. El formulario de contacto es Contact.
    public partial class StudentDetailViewModel : BusyStateViewModel, IQueryAttributable
    {
        public const string StudentQueryKey = "Student";
        public const string SectionQueryKey = "SectionName";
        public const string SectionIdQueryKey = "SectionId";

        private const string LoadFailedMessage = "No se pudo cargar el detalle del alumno.";

        private readonly IStudentDetailsService _detailsService;
        private readonly IAppNavigation _navigation;
        private int _studentId;
        private int? _sectionId;

        public StudentDetailViewModel(IStudentDetailsService detailsService, IAppNavigation navigation, IClock clock)
        {
            _detailsService = detailsService;
            _navigation = navigation;
            Contact = new ContactFormViewModel(detailsService, clock, () => _studentId, ReloadAsync);
        }

        public ContactFormViewModel Contact { get; }

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

        public ObservableCollection<HistoryItem> Attendance { get; } = new();

        public ObservableCollection<HistoryItem> Submissions { get; } = new();

        public ObservableCollection<HistoryItem> Interventions { get; } = new();

        // Estados vacíos: solo cuando ya terminó de cargar.
        public bool HasNoAttendance => !IsBusy && Attendance.Count == 0;

        public bool HasNoSubmissions => !IsBusy && Submissions.Count == 0;

        public bool HasNoInterventions => !IsBusy && Interventions.Count == 0;

        public string HeaderSubtitle => string.IsNullOrEmpty(StudentNumber)
            ? string.Empty
            : $"{PersonName.AsGivenFirst(Name)} · {StudentNumber}";

        public int Attended { get; private set; }

        public int ClassTotal { get; private set; }

        public int Submitted { get; private set; }

        public int AssignmentTotal { get; private set; }

        public double AttendanceRatio => ClassTotal == 0 ? 0 : (double)Attended / ClassTotal;

        public double SubmissionRatio => AssignmentTotal == 0 ? 0 : (double)Submitted / AssignmentTotal;

        public string AttendanceCaption => $"{Attended} de {ClassTotal} · {Percent(AttendanceRatio)}";

        public string SubmissionCaption => $"{Submitted} de {AssignmentTotal} · {Percent(SubmissionRatio)}";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var parsed = StudentDetailQuery.Read(query);
            if (parsed.SectionName is { Length: > 0 } sectionName)
                Commission = sectionName;

            if (parsed.Row is not { } row)
                return;

            ShowRow(row);
            _sectionId = parsed.SectionId;
            _studentId = row.Student.Id;
            _ = ReloadAsync();
        }

        protected override void OnBusyChanged()
        {
            OnPropertyChanged(nameof(HasNoAttendance));
            OnPropertyChanged(nameof(HasNoSubmissions));
            OnPropertyChanged(nameof(HasNoInterventions));
        }

        // Datos de la fila: se muestran enseguida y el riesgo queda igual que en la grilla.
        private void ShowRow(StudentRow row)
        {
            Name = row.Name;
            StudentNumber = row.StudentNumber;
            Phone = row.Phone;
            Email = row.Email;
            RiskLevel = row.RiskLevel;
            RiskScore = row.RiskScore;
            Justification = row.Justification;
            if (row.Risk is { } risk)
            {
                Attended = risk.Data.AttendedClasses;
                ClassTotal = risk.Data.TotalClasses;
                Submitted = risk.Data.SubmittedAssignments;
                AssignmentTotal = risk.Data.TotalAssignments;
                NotifyMetrics();
            }

            BadgeBackground = row.BadgeBackground;
            BadgeForeground = row.BadgeForeground;
        }

        private Task ReloadAsync() =>
            RunLoopAsync(() => LoadDetailAsync(_studentId, _sectionId), LoadFailedMessage);

        private async Task LoadDetailAsync(int studentId, int? sectionId)
        {
            Attendance.Clear();
            Submissions.Clear();
            Interventions.Clear();
            var detail = await _detailsService.GetAsync(studentId, sectionId);
            if (detail is null)
            {
                ErrorMessage = "No se encontró el alumno.";
                return;
            }

            ShowDetail(detail, sectionId);
            foreach (var record in detail.AttendanceRecords)
                Attendance.Add(HistoryFormatter.Attendance(record));
            foreach (var submission in detail.Submissions)
                Submissions.Add(HistoryFormatter.Submission(submission));
            foreach (var intervention in detail.Interventions)
                Interventions.Add(HistoryFormatter.Intervention(intervention));
        }

        private void ShowDetail(StudentDetail detail, int? sectionId)
        {
            Phone = detail.PhoneNumber;
            Email = detail.Email;
            StudentNumber = detail.StudentNumber;
            if (string.IsNullOrEmpty(Commission) || sectionId is null || sectionId == detail.SectionId)
                Commission = detail.SectionName;

            Name = PersonName.AsSurnameFirst($"{detail.Name} {detail.LastName}");
            OnPropertyChanged(nameof(HeaderSubtitle));
        }

        private static string Percent(double ratio) => $"{Math.Round(ratio * 100)}%";

        private void NotifyMetrics()
        {
            OnPropertyChanged(nameof(Attended));
            OnPropertyChanged(nameof(ClassTotal));
            OnPropertyChanged(nameof(Submitted));
            OnPropertyChanged(nameof(AssignmentTotal));
            OnPropertyChanged(nameof(AttendanceRatio));
            OnPropertyChanged(nameof(SubmissionRatio));
            OnPropertyChanged(nameof(AttendanceCaption));
            OnPropertyChanged(nameof(SubmissionCaption));
            OnPropertyChanged(nameof(HeaderSubtitle));
        }

        partial void OnNameChanged(string value) => OnPropertyChanged(nameof(HeaderSubtitle));

        partial void OnStudentNumberChanged(string value) => OnPropertyChanged(nameof(HeaderSubtitle));

        [RelayCommand]
        private Task BackAsync() => _navigation.GoToStudentsAsync();
    }
}
