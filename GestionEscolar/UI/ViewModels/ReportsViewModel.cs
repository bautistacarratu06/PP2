using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GestionEscolar.Core.Models;
using GestionEscolar.Core.Services;
using UI.Services;

namespace UI.ViewModels
{
    // Pantalla de Reportes: genera el texto de riesgo de la comisión y permite exportarlo a .txt.
    public partial class ReportsViewModel : SectionScopedViewModel
    {
        private readonly IReportService _reports;
        private readonly IReportShareService _share;

        public ReportsViewModel(
            SectionSelection selection,
            IReportService reports,
            IReportShareService share) : base(selection)
        {
            _reports = reports ?? throw new ArgumentNullException(nameof(reports));
            _share = share ?? throw new ArgumentNullException(nameof(share));
        }

        public ObservableCollection<string> Paragraphs { get; } = new();

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(HasParagraphs))]
        [NotifyPropertyChangedFor(nameof(CanExport))]
        [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
        public partial SectionRiskReport? CurrentReport { get; set; }

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CanExport))]
        [NotifyCanExecuteChangedFor(nameof(ExportCommand))]
        public partial bool IsExporting { get; set; }

        public bool HasParagraphs => Paragraphs.Count > 0;

        public bool CanExport =>
            CurrentReport is { HasData: true } && !IsBusy && !IsExporting;

        protected override void OnBusyChanged()
        {
            OnPropertyChanged(nameof(CanExport));
            ExportCommand.NotifyCanExecuteChanged();
        }

        protected override async Task OnSectionChangedAsync()
        {
            Paragraphs.Clear();
            CurrentReport = null;
            StatusMessage = string.Empty;

            if (SelectedSection is null)
                return;

            var report = await _reports.GenerateReportAsync(
                SelectedSection.Id,
                SelectedSection.Name);

            if (HasPendingWork)
                return;

            CurrentReport = report;
            foreach (var paragraph in report.Paragraphs)
                Paragraphs.Add(paragraph);
        }

        [RelayCommand(CanExecute = nameof(CanExport))]
        private async Task ExportAsync()
        {
            if (CurrentReport is null || IsExporting)
                return;

            ErrorMessage = string.Empty;
            StatusMessage = string.Empty;
            IsExporting = true;
            try
            {
                var text = _reports.FormatExportText(CurrentReport);
                var fileName = _reports.BuildExportFileName(
                    CurrentReport.SectionName,
                    CurrentReport.GeneratedAt);
                await _share.ShareTextFileAsync(fileName, text);
                StatusMessage = "Reporte listo para guardar o compartir.";
            }
            catch (Exception ex)
            {
                ReportError("No se pudo exportar el reporte.", ex);
            }
            finally
            {
                IsExporting = false;
            }
        }
    }
}
