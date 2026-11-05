using System.Windows.Input;

namespace UI.Controls;

public partial class ConfirmationModal : ContentView
{
    public static readonly BindableProperty IsOpenProperty =
        BindableProperty.Create(nameof(IsOpen), typeof(bool), typeof(ConfirmationModal), false);

    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(ConfirmationModal), "Confirmar acción");

    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(string), typeof(ConfirmationModal), string.Empty);

    public static readonly BindableProperty ConfirmTextProperty =
        BindableProperty.Create(nameof(ConfirmText), typeof(string), typeof(ConfirmationModal), "Aceptar");

    public static readonly BindableProperty CancelTextProperty =
        BindableProperty.Create(nameof(CancelText), typeof(string), typeof(ConfirmationModal), "Cancelar");

    public static readonly BindableProperty ConfirmCommandProperty =
        BindableProperty.Create(nameof(ConfirmCommand), typeof(ICommand), typeof(ConfirmationModal));

    public static readonly BindableProperty CancelCommandProperty =
        BindableProperty.Create(nameof(CancelCommand), typeof(ICommand), typeof(ConfirmationModal));

    public ConfirmationModal()
    {
        InitializeComponent();
    }

    public bool IsOpen
    {
        get => (bool)GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Message
    {
        get => (string)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public string ConfirmText
    {
        get => (string)GetValue(ConfirmTextProperty);
        set => SetValue(ConfirmTextProperty, value);
    }

    public string CancelText
    {
        get => (string)GetValue(CancelTextProperty);
        set => SetValue(CancelTextProperty, value);
    }

    public ICommand? ConfirmCommand
    {
        get => (ICommand?)GetValue(ConfirmCommandProperty);
        set => SetValue(ConfirmCommandProperty, value);
    }

    public ICommand? CancelCommand
    {
        get => (ICommand?)GetValue(CancelCommandProperty);
        set => SetValue(CancelCommandProperty, value);
    }
}

