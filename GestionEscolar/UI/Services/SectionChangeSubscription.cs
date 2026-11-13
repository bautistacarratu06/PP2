namespace UI.Services;

// Suscripción a SectionSelection.Changed con vida de pantalla: se activa al aparecer la página
// y se suelta al desaparecer. Subscribe es idempotente, así que reaparecer no duplica el handler.
public sealed class SectionChangeSubscription
{
    private readonly SectionSelection _selection;
    private readonly Action _onChanged;
    private readonly IMainThreadDispatcher _dispatcher;
    private bool _subscribed;

    public SectionChangeSubscription(SectionSelection selection, Action onChanged, IMainThreadDispatcher? dispatcher = null)
    {
        _selection = selection;
        _onChanged = onChanged;
        _dispatcher = dispatcher ?? new MauiMainThreadDispatcher();
    }

    public void Subscribe()
    {
        if (_subscribed)
            return;

        _selection.Changed += OnSelectionChanged;
        _subscribed = true;
    }

    public void Unsubscribe()
    {
        if (!_subscribed)
            return;

        _selection.Changed -= OnSelectionChanged;
        _subscribed = false;
    }

    private void OnSelectionChanged(object? sender, EventArgs e) =>
        _dispatcher.Post(_onChanged);
}
