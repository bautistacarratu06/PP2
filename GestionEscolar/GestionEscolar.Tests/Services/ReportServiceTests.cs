using GestionEscolar.Core.Enums;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using GestionEscolar.Tests.Fakes;

namespace GestionEscolar.Tests.Services;

public class ReportServiceTests
{
    private static StudentTrackingData Row(
        string last,
        string first,
        string number,
        int attended,
        int totalClasses,
        int submitted,
        int totalAssignments,
        bool active = true) =>
        new()
        {
            Student = new Student
            {
                LastName = last,
                FirstName = first,
                Name = first + " " + last,
                StudentNumber = number,
                IsActive = active
            },
            AttendedClasses = attended,
            TotalClasses = totalClasses,
            SubmittedAssignments = submitted,
            TotalAssignments = totalAssignments
        };

    [Fact]
    public async Task Given_EmptySection_When_GeneratingReport_Then_ReturnsNoData()
    {
        var repo = new FakeTrackingRepository();
        var service = new ReportService(repo, new RiskConfig());

        var report = await service.GenerateReportAsync(1, "Comisión A");

        Assert.False(report.HasData);
        Assert.Equal(0, report.TotalStudents);
        Assert.Equal(0, report.AtRiskCount);
        Assert.Contains("No se encontraron estudiantes", report.Paragraphs[0]);
    }

    [Fact]
    public async Task Given_StudentsWithoutAttendanceOrAssignments_When_GeneratingReport_Then_ReturnsExactNoDataMessage()
    {
        // CA3: Si una comisión tiene estudiantes cargados pero cero asistencias y entregas registradas,
        // el reporte debe indicar textualmente: "Todavía no hay datos para calcular el riesgo".
        // Bajo ninguna circunstancia el sistema debe mostrar "0 en riesgo" si no hay historial previo que sustente ese cálculo.
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("Pérez", "Juan", "101", attended: 0, totalClasses: 0, submitted: 0, totalAssignments: 0),
            Row("Gómez", "María", "102", attended: 0, totalClasses: 0, submitted: 0, totalAssignments: 0));

        var service = new ReportService(repo, new RiskConfig());
        var report = await service.GenerateReportAsync(1, "Comisión 1A");

        Assert.False(report.HasData);
        Assert.Equal(SectionRiskReport.NoDataMessage, report.StatusMessage);
        Assert.Contains(SectionRiskReport.NoDataMessage, report.Paragraphs[0]);
    }

    [Fact]
    public async Task Given_StudentsWithData_When_GeneratingReport_Then_FormatsRiskRatioAndStatistics()
    {
        // CA1: El reporte debe indicar claramente el número de alumnos en riesgo sobre el total (ej. "7 de 30 estudiantes están en riesgo de abandonar").
        var repo = new FakeTrackingRepository().WithSection(1,
            // Riesgo alto (score >= 70): 0/10 asistencias y 0/5 entregas
            Row("Riesgoso", "Uno", "101", attended: 0, totalClasses: 10, submitted: 0, totalAssignments: 5),
            Row("Riesgoso", "Dos", "102", attended: 1, totalClasses: 10, submitted: 0, totalAssignments: 5),
            // Riesgo bajo (score < 40): 10/10 asistencias y 5/5 entregas
            Row("Cumplidor", "Tres", "103", attended: 10, totalClasses: 10, submitted: 5, totalAssignments: 5));

        var service = new ReportService(repo, new RiskConfig());
        var report = await service.GenerateReportAsync(1, "Comisión Noche");

        Assert.True(report.HasData);
        Assert.Equal(3, report.TotalStudents);
        Assert.Equal(2, report.AtRiskCount);
        Assert.Equal(2, report.RiskSummary.High);
        Assert.Equal(1, report.RiskSummary.Low);

        Assert.Contains("2 de 3 estudiantes están en riesgo de abandonar", report.Paragraphs[0]);
        Assert.Contains("asistencia promedio", report.Paragraphs[1]);
        Assert.Contains("entrega de trabajos", report.Paragraphs[1]);
    }

    [Fact]
    public async Task Given_ActiveAndInactiveStudents_When_GeneratingReport_Then_IgnoresInactive()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("Activo", "A", "101", attended: 0, totalClasses: 10, submitted: 0, totalAssignments: 5, active: true),
            Row("Inactivo", "I", "102", attended: 0, totalClasses: 10, submitted: 0, totalAssignments: 5, active: false));

        var service = new ReportService(repo, new RiskConfig());
        var report = await service.GenerateReportAsync(1, "Comisión Mañana");

        Assert.True(report.HasData);
        Assert.Equal(1, report.TotalStudents);
        Assert.Equal(1, report.AtRiskCount);
    }

    [Theory]
    [InlineData(10, 5)]
    [InlineData(0, 0)]
    public async Task Given_StudentsWithContacts_When_GeneratingReport_Then_CountsEveryContactOfTheSection(int totalClasses, int totalAssignments)
    {
        // Con historial (reporte con datos) y sin historial (sin datos) el contador es el mismo.
        var active = Row("Activo", "A", "101", attended: 0, totalClasses, submitted: 0, totalAssignments);
        active.ContactCount = 3;
        var inactive = Row("Inactivo", "I", "102", attended: 0, totalClasses, submitted: 0, totalAssignments, active: false);
        inactive.ContactCount = 2;
        var repo = new FakeTrackingRepository().WithSection(1, active, inactive);

        var report = await new ReportService(repo, new RiskConfig()).GenerateReportAsync(1, "Comisión Mañana");

        Assert.Equal(5, report.InterventionCount);
    }

    [Fact]
    public async Task Given_StudentsEnrolledAtDifferentTimes_When_GeneratingReport_Then_CountsClassesAndActiveDeliveries()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("Desde", "Inicio", "101", attended: 8, totalClasses: 10, submitted: 3, totalAssignments: 4),
            Row("Tarde", "Llegó", "102", attended: 2, totalClasses: 6, submitted: 1, totalAssignments: 2),
            Row("Baja", "Dio", "103", attended: 0, totalClasses: 10, submitted: 0, totalAssignments: 4, active: false));

        var report = await new ReportService(repo, new RiskConfig()).GenerateReportAsync(1, "Comisión Mañana");

        Assert.Equal(10, report.ClassCount);
        Assert.Equal(4, report.SubmittedCount);
        Assert.Equal(6, report.ExpectedSubmissions);
    }

    [Fact]
    public async Task Given_Section_When_ComparingReportAndStudentsQueryService_Then_SummariesMatchExactly()
    {
        // CA2: El número de estudiantes en riesgo mostrado en el reporte debe coincidir exactamente con el mostrado en el panel de Seguimiento.
        // Ambos ViewModels deben consumir el mismo método o servicio centralizado.
        var config = new RiskConfig();
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("Uno", "A", "1", 2, 10, 1, 5, active: true),
            Row("Dos", "B", "2", 8, 10, 4, 5, active: true),
            Row("Tres", "C", "3", 10, 10, 5, 5, active: true),
            Row("Inactivo", "D", "4", 0, 10, 0, 5, active: false));

        var reportService = new ReportService(repo, config);
        var queryService = new StudentsQueryService(repo, config, reportService);

        var report = await reportService.GenerateReportAsync(1, "Comisión Test");
        var queryResult = await queryService.QueryAsync(1);

        Assert.Equal(queryResult.ActiveRiskSummary.High, report.RiskSummary.High);
        Assert.Equal(queryResult.ActiveRiskSummary.Medium, report.RiskSummary.Medium);
        Assert.Equal(queryResult.ActiveRiskSummary.Low, report.RiskSummary.Low);
        Assert.Equal(queryResult.ActiveRiskSummary.Total, report.RiskSummary.Total);
    }

    [Fact]
    public async Task Given_ReportWithData_When_FormattingExportText_Then_IncludesHeaderSectionDateAndRiskRatio()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("Riesgoso", "Uno", "101", attended: 0, totalClasses: 10, submitted: 0, totalAssignments: 5),
            Row("Cumplidor", "Dos", "102", attended: 10, totalClasses: 10, submitted: 5, totalAssignments: 5));
        var service = new ReportService(repo, new RiskConfig());
        var report = await service.GenerateReportAsync(1, "Comisión Noche");

        var text = service.FormatExportText(report);

        Assert.Contains("GESTIÓN ESCOLAR", text);
        Assert.Contains("Comisión: Comisión Noche", text);
        Assert.Contains("Emitido:", text);
        Assert.Contains("1 de 2 estudiantes está en riesgo de abandonar", text);
        Assert.Contains(report.Paragraphs[0], text);
        Assert.Contains(report.Paragraphs[1], text);
    }

    [Fact]
    public async Task Given_ReportWithoutHistory_When_FormattingExportText_Then_IncludesExactNoDataMessage()
    {
        var repo = new FakeTrackingRepository().WithSection(1,
            Row("Pérez", "Juan", "101", attended: 0, totalClasses: 0, submitted: 0, totalAssignments: 0));
        var service = new ReportService(repo, new RiskConfig());
        var report = await service.GenerateReportAsync(1, "Comisión Vacía");

        var text = service.FormatExportText(report);

        Assert.Contains("GESTIÓN ESCOLAR", text);
        Assert.Contains("Comisión: Comisión Vacía", text);
        Assert.Contains(SectionRiskReport.NoDataMessage, text);
        Assert.DoesNotContain("están en riesgo de abandonar", text);
    }

    [Fact]
    public void Given_SectionName_When_BuildingExportFileName_Then_UsesSafeNameAndTxtExtension()
    {
        var service = new ReportService(new FakeTrackingRepository(), new RiskConfig());
        var name = service.BuildExportFileName("Comisión 1A / Mañana", new DateTime(2026, 11, 13, 15, 30, 0));

        Assert.Equal("Reporte_Comision_Comision_1A_Manana_20261113_1530.txt", name);
        Assert.EndsWith(".txt", name);
    }
}

