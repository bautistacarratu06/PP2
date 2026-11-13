namespace UI.Services;

// Despacho al hilo de UI para que las suscripciones a eventos de la plataforma
// puedan ejecutarse en tests sin inicializar el runtime de MAUI.
public interface IMainThreadDispatcher
{
    void Post(Action action);
}

public sealed class MauiMainThreadDispatcher : IMainThreadDispatcher
{
    public void Post(Action action) => MainThread.BeginInvokeOnMainThread(action);
}

public sealed class ImmediateMainThreadDispatcher : IMainThreadDispatcher
{
    public void Post(Action action) => action();
}
