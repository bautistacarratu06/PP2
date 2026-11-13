using GestionEscolar.Core.Models;
using UI.Services;

namespace UI.Controls;

public partial class SectionPicker : ContentView
{
    private SectionSelection? _selection;
    private bool _syncing;

    public SectionPicker()
    {
        InitializeComponent();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (_selection is not null || Handler?.MauiContext?.Services.GetService<SectionSelection>() is not { } selection)
            return;

        _selection = selection;
        _selection.Changed += OnSelectionChanged;
        _ = LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_selection is null)
            return;

        try
        {
            await _selection.EnsureLoadedAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"No se pudieron cargar las comisiones: {ex}");
            return;
        }

        MainThread.BeginInvokeOnMainThread(Sync);
    }

    private void OnSelectionChanged(object? sender, EventArgs e) =>
        MainThread.BeginInvokeOnMainThread(Sync);

    private void Sync()
    {
        if (_selection is null)
            return;

        _syncing = true;
        Picker.ItemsSource = _selection.Items.ToList();
        Picker.SelectedItem = _selection.Items.FirstOrDefault(section => section.Id == _selection.Current?.Id);
        ValueLabel.Text = _selection.Current?.Name ?? string.Empty;
        _syncing = false;
    }

    private void OnSelected(object? sender, EventArgs e)
    {
        if (_syncing || _selection is null || Picker.SelectedItem is not Section section)
            return;

        ValueLabel.Text = section.Name;
        _selection.Select(section);
    }
}
