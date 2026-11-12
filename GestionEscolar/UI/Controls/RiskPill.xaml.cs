using UI.Formatting;

namespace UI.Controls;

public partial class RiskPill : ContentView
{
    public static readonly BindableProperty LevelProperty =
        BindableProperty.Create(nameof(Level), typeof(string), typeof(RiskPill), string.Empty, propertyChanged: OnChanged);

    public static readonly BindableProperty ScoreProperty =
        BindableProperty.Create(nameof(Score), typeof(int?), typeof(RiskPill), null, propertyChanged: OnChanged);

    public RiskPill()
    {
        InitializeComponent();
        Apply();
    }

    public string Level
    {
        get => (string)GetValue(LevelProperty);
        set => SetValue(LevelProperty, value);
    }

    public int? Score
    {
        get => (int?)GetValue(ScoreProperty);
        set => SetValue(ScoreProperty, value);
    }

    private static void OnChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((RiskPill)bindable).Apply();

    private void Apply()
    {
        var (color, soft) = RiskPalette.For(Level);
        var ink = Color.FromArgb(color);
        Capsule.BackgroundColor = Color.FromArgb(soft);
        Dot.Fill = ink;
        LevelLabel.Text = Level;
        LevelLabel.TextColor = ink;
        ScoreLabel.IsVisible = Score is not null;
        ScoreLabel.Text = Score?.ToString();
    }
}
