using System.Windows.Input;

namespace UI.Controls;

public partial class FloatingActionBar : ContentView
{
    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(FloatingActionBar), false);

    public static readonly BindableProperty SelectedCountProperty =
        BindableProperty.Create(nameof(SelectedCount), typeof(int), typeof(FloatingActionBar), 0);

    public static readonly BindableProperty SelectedCountTextProperty =
        BindableProperty.Create(nameof(SelectedCountText), typeof(string), typeof(FloatingActionBar), string.Empty);

    public static readonly BindableProperty ClearSelectionCommandProperty =
        BindableProperty.Create(nameof(ClearSelectionCommand), typeof(ICommand), typeof(FloatingActionBar));

    public static readonly BindableProperty ActionsContentProperty =
        BindableProperty.Create(nameof(ActionsContent), typeof(View), typeof(FloatingActionBar));

    public FloatingActionBar()
    {
        InitializeComponent();
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public int SelectedCount
    {
        get => (int)GetValue(SelectedCountProperty);
        set => SetValue(SelectedCountProperty, value);
    }

    public string SelectedCountText
    {
        get => (string)GetValue(SelectedCountTextProperty);
        set => SetValue(SelectedCountTextProperty, value);
    }

    public ICommand? ClearSelectionCommand
    {
        get => (ICommand?)GetValue(ClearSelectionCommandProperty);
        set => SetValue(ClearSelectionCommandProperty, value);
    }

    public View? ActionsContent
    {
        get => (View?)GetValue(ActionsContentProperty);
        set => SetValue(ActionsContentProperty, value);
    }
}

