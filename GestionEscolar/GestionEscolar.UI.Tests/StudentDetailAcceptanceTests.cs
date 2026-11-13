using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;
using UI.Services;
using UI.ViewModels;

namespace GestionEscolar.UI.Tests;

// Aceptación del detalle del alumno: el ViewModel real con StudentDetailsService sobre el
// repositorio en memoria. Shell solo aparece a través de IAppNavigation, reemplazado acá.
public sealed class StudentDetailAcceptanceTests
{
    private const int StudentId = 7;
    private const int SectionId = 1;
    private const string SectionName = "Comisión A";
    private static readonly DateTime Today = new(2026, 11, 13);

    private readonly FakeStudentDetailsRepository _repository = new();
    private readonly RecordingNavigation _navigation = new();
    private readonly StudentDetailViewModel _viewModel;

    public StudentDetailAcceptanceTests()
    {
        _repository.Students[StudentId] = new StudentDetail
        {
            StudentId = StudentId,
            Name = "Ana",
            LastName = "Pérez",
            StudentNumber = "L-7",
            Email = "ana@edu.test",
            PhoneNumber = "1155550000",
            SectionId = SectionId,
            SectionName = SectionName
        };
        _viewModel = new StudentDetailViewModel(new StudentDetailsService(_repository), _navigation, new FixedClock(Today));
    }

    [Fact]
    public async Task Given_ARowFromTheGrid_When_Opening_Then_HeaderRiskColorsAndCountsAreShown()
    {
        SeedHistories();

        await OpenAsync(Row(RiskLevel.Alto, riskScore: 70, attended: 3, classes: 4, submitted: 1, assignments: 2));

        Assert.False(_viewModel.HasError);
        Assert.Equal("Pérez, Ana", _viewModel.Name);
        Assert.Equal("Ana Pérez · L-7", _viewModel.HeaderSubtitle);
        Assert.Equal(SectionName, _viewModel.Commission);
        Assert.Equal("1155550000", _viewModel.Phone);
        Assert.Equal("ana@edu.test", _viewModel.Email);
        Assert.Equal("Alto", _viewModel.RiskLevel);
        Assert.Equal(70, _viewModel.RiskScore);
        var (background, foreground) = RiskBadge.For(RiskLevel.Alto);
        Assert.Equal(background, _viewModel.BadgeBackground);
        Assert.Equal(foreground, _viewModel.BadgeForeground);
        Assert.Equal("3 de 4 · 75%", _viewModel.AttendanceCaption);
        Assert.Equal("1 de 2 · 50%", _viewModel.SubmissionCaption);
    }

    [Fact]
    public async Task Given_SeededHistories_When_Opening_Then_TheThreeListsAreFilled()
    {
        SeedHistories();

        await OpenAsync(Row(RiskLevel.Medio, riskScore: 40, attended: 1, classes: 2, submitted: 0, assignments: 1));

        Assert.Equal(
            [new HistoryItem("Clase · 03/11/2026", "Presente", "success"), new HistoryItem("Clase · 10/11/2026", "Ausente", "danger")],
            _viewModel.Attendance);
        Assert.Equal([new HistoryItem("TP 1 · vence 05/11/2026", "No entregó", "danger")], _viewModel.Submissions);
        Assert.Equal([new HistoryItem("01/11/2026 · Correo · No contestó", string.Empty, "neutral")], _viewModel.Interventions);
        Assert.False(_viewModel.HasNoAttendance);
        Assert.False(_viewModel.HasNoSubmissions);
        Assert.False(_viewModel.HasNoInterventions);
    }

    [Fact]
    public async Task Given_NoHistory_When_Opening_Then_EveryEmptyStateIsShown()
    {
        await OpenAsync(Row(RiskLevel.Bajo, riskScore: 0, attended: 0, classes: 0, submitted: 0, assignments: 0));

        Assert.True(_viewModel.HasNoAttendance);
        Assert.True(_viewModel.HasNoSubmissions);
        Assert.True(_viewModel.HasNoInterventions);
        Assert.Equal("0 de 0 · 0%", _viewModel.AttendanceCaption);
    }

    [Fact]
    public async Task Given_AStudentThatNoLongerExists_When_Opening_Then_AnErrorIsShown()
    {
        _repository.Students.Clear();

        await OpenAsync(Row(RiskLevel.Bajo, riskScore: 0, attended: 0, classes: 0, submitted: 0, assignments: 0));

        Assert.Equal("No se encontró el alumno.", _viewModel.ErrorMessage);
    }

    [Fact]
    public async Task Given_AValidContact_When_Saving_Then_ItAppearsInInterventionsAndTheFormCloses()
    {
        await OpenAsync(Row(RiskLevel.Alto, riskScore: 70, attended: 0, classes: 1, submitted: 0, assignments: 1));
        var contact = _viewModel.Contact;

        contact.OpenContactCommand.Execute(null);
        contact.SelectedChannel = ContactChannel.Llamada.ToDbValue();
        contact.SelectedOutcome = ContactOutcome.SinNovedades.ToDbValue();
        contact.Note = "  Atendió la madre  ";
        await contact.SaveContactCommand.ExecuteAsync(null);
        await IdleAsync();

        Assert.False(contact.IsContactOpen);
        Assert.False(contact.HasContactError);
        Assert.False(_viewModel.HasNoInterventions);
        var saved = Assert.Single(_viewModel.Interventions);
        Assert.Equal("13/11/2026 · Llamada · Contactado - sin novedades (Atendió la madre)", saved.Label);
        Assert.Equal(StudentId, Assert.Single(_repository.Interventions).StudentId);
    }

    [Fact]
    public async Task Given_AContactWithoutChannel_When_Saving_Then_TheErrorIsShownAndNothingIsStored()
    {
        await OpenAsync(Row(RiskLevel.Alto, riskScore: 70, attended: 0, classes: 1, submitted: 0, assignments: 1));
        var contact = _viewModel.Contact;

        contact.OpenContactCommand.Execute(null);
        contact.SelectedOutcome = ContactOutcome.NoContesto.ToDbValue();
        await contact.SaveContactCommand.ExecuteAsync(null);

        Assert.True(contact.HasContactError);
        Assert.Equal("El medio no puede estar vacío.", contact.ContactError);
        Assert.True(contact.IsContactOpen);
        Assert.Empty(_viewModel.Interventions);
        Assert.Empty(_repository.Interventions);
    }

    [Fact]
    public async Task Given_AContactDatedInTheFuture_When_Saving_Then_TheErrorIsShownAndNothingIsStored()
    {
        await OpenAsync(Row(RiskLevel.Alto, riskScore: 70, attended: 0, classes: 1, submitted: 0, assignments: 1));
        var contact = _viewModel.Contact;

        contact.OpenContactCommand.Execute(null);
        contact.ContactDate = Today.AddDays(1);
        contact.SelectedChannel = ContactChannel.WhatsApp.ToDbValue();
        contact.SelectedOutcome = ContactOutcome.ComprometioVolver.ToDbValue();
        await contact.SaveContactCommand.ExecuteAsync(null);

        Assert.Equal("La fecha no puede ser posterior a hoy.", contact.ContactError);
        Assert.Empty(_repository.Interventions);
    }

    [Fact]
    public async Task Given_TheDetailScreen_When_GoingBack_Then_ItNavigatesToStudents()
    {
        await OpenAsync(Row(RiskLevel.Bajo, riskScore: 0, attended: 0, classes: 0, submitted: 0, assignments: 0));

        await _viewModel.BackCommand.ExecuteAsync(null);

        Assert.Equal(1, _navigation.StudentsVisits);
    }

    private async Task OpenAsync(StudentRow row)
    {
        _viewModel.ApplyQueryAttributes(new Dictionary<string, object>
        {
            [StudentDetailViewModel.StudentQueryKey] = row,
            [StudentDetailViewModel.SectionQueryKey] = SectionName,
            [StudentDetailViewModel.SectionIdQueryKey] = SectionId
        });
        await IdleAsync();
    }

    // La carga se lanza sin await desde ApplyQueryAttributes; se espera a que el ViewModel quede libre.
    private async Task IdleAsync()
    {
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (_viewModel.IsBusy)
        {
            if (DateTime.UtcNow > deadline)
                throw new TimeoutException("El detalle del alumno no terminó de cargar.");
            await Task.Delay(10);
        }
    }

    private void SeedHistories()
    {
        _repository.ClassSessionSection[100] = SectionId;
        _repository.ClassSessionSection[101] = SectionId;
        _repository.Attendance.Add(new AttendanceRecord { StudentId = StudentId, ClassSessionId = 100, Date = new DateTime(2026, 11, 3), Status = AttendanceStatus.Presente });
        _repository.Attendance.Add(new AttendanceRecord { StudentId = StudentId, ClassSessionId = 101, Date = new DateTime(2026, 11, 10), Status = AttendanceStatus.Ausente });
        _repository.Submissions.Add(new Submission { StudentId = StudentId, SectionId = SectionId, AssignmentName = "TP 1", DueDate = new DateTime(2026, 11, 5), Status = SubmissionStatus.NoEntrego });
        _repository.Interventions.Add(new Intervention
        {
            Id = 1,
            StudentId = StudentId,
            Date = new DateTime(2026, 11, 1),
            Channel = ContactChannel.Correo.ToDbValue(),
            Outcome = ContactOutcome.NoContesto.ToDbValue()
        });
    }

    private static StudentRow Row(RiskLevel level, int riskScore, int attended, int classes, int submitted, int assignments)
    {
        var student = new Student
        {
            Id = StudentId,
            SectionId = SectionId,
            Name = "Ana Pérez",
            FirstName = "Ana",
            LastName = "Pérez",
            StudentNumber = "L-7",
            Phone = "1155550000",
            Email = "ana@edu.test"
        };
        var data = new StudentTrackingData
        {
            Student = student,
            AttendedClasses = attended,
            TotalClasses = classes,
            SubmittedAssignments = submitted,
            TotalAssignments = assignments
        };
        return new StudentRow(new StudentRiskResult(data, riskScore, level), rowNumber: 1);
    }

    private sealed class FixedClock(DateTime today) : IClock
    {
        public DateTime Today { get; } = today;
    }

    private sealed class RecordingNavigation : IAppNavigation
    {
        public int StudentsVisits { get; private set; }

        public Task GoToStudentsAsync()
        {
            StudentsVisits++;
            return Task.CompletedTask;
        }
    }
}
