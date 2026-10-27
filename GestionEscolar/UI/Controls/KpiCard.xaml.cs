using System.Windows.Input;

namespace UI.Controls;

public partial class KpiCard : ContentView
{
    public static readonly BindableProperty CaptionTextProperty =
        BindableProperty.Create(nameof(CaptionText), typeof(string), typeof(KpiCard), string.Empty, propertyChanged: Refresh);

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(int), typeof(KpiCard), 0, propertyChanged: Refresh);

    public static readonly BindableProperty TotalProperty =
        BindableProperty.Create(nameof(Total), typeof(int), typeof(KpiCard), 0, propertyChanged: Refresh);

    public static readonly BindableProperty LevelProperty =
        BindableProperty.Create(nameof(Level), typeof(string), typeof(KpiCard), "Bajo", propertyChanged: Refresh);

    public static readonly BindableProperty IsActiveProperty =
        BindableProperty.Create(nameof(IsActive), typeof(bool), typeof(KpiCard), false, propertyChanged: Refresh);

    public static readonly BindableProperty CommandProperty =
        BindableProperty.Create(nameof(Command), typeof(ICommand), typeof(KpiCard));

    public static readonly BindableProperty CommandParameterProperty =
        BindableProperty.Create(nameof(CommandParameter), typeof(object), typeof(KpiCard));

    public KpiCard()
    {
        InitializeComponent();
        Apply();
    }

    public string CaptionText
    {
        get => (string)GetValue(CaptionTextProperty);
        set => SetValue(CaptionTextProperty, value);
    }

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public int Total
    {
        get => (int)GetValue(TotalProperty);
        set => SetValue(TotalProperty, value);
    }

    public string Level
    {
        get => (string)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    private static void Refresh(BindableObject bindable, object oldValue, object newValue) =>
        ((KpiCard)bindable).Apply();

    private void Apply()
    {
        var (ink, soft) = Level switch
        {
            "Alto" => ("#F87171", "#1FF87171"),
            "Medio" => ("#FBBF24", "#1FFBBF24"),
            _ => ("#34D399", "#1F34D399")
        };
        Caption.Text = CaptionText;
        ValueLabel.Text = Value.ToString();
        TotalLabel.Text = $"de {Total}";
        Dot.Fill = Color.FromArgb(ink);
        Chrome.Stroke = IsActive ? Color.FromArgb(ink) : Color.FromArgb("#232323");
        Chrome.Background = IsActive
            ? new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(0, 1),
                GradientStops =
                {
                    new GradientStop(Color.FromArgb(soft), 0),
                    new GradientStop(Color.FromArgb("#131313"), 0.7f)
                }
            }
            : new SolidColorBrush(Color.FromArgb("#131313"));
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (Command?.CanExecute(CommandParameter) == true)
            Command.Execute(CommandParameter);
    }
}
