using Microsoft.Maui.Controls.Shapes;

namespace UI.Controls;

public class StatusPill : Border
{
    public static readonly BindableProperty TextProperty =
        BindableProperty.Create(nameof(Text), typeof(string), typeof(StatusPill), string.Empty, propertyChanged: OnChanged);

    public static readonly BindableProperty KindProperty =
        BindableProperty.Create(nameof(Kind), typeof(string), typeof(StatusPill), "neutral", propertyChanged: OnChanged);

    private readonly Ellipse _dot = new() { WidthRequest = 6, HeightRequest = 6, VerticalOptions = LayoutOptions.Center };
    private readonly Label _label = new()
    {
        FontFamily = "InterSemibold",
        FontSize = 12,
        VerticalOptions = LayoutOptions.Center
    };

    public StatusPill()
    {
        StrokeThickness = 0;
        StrokeShape = new RoundRectangle { CornerRadius = 999 };
        Padding = new Thickness(9, 4);
        HorizontalOptions = LayoutOptions.End;
        VerticalOptions = LayoutOptions.Center;
        Content = new HorizontalStackLayout
        {
            Spacing = 6,
            Children = { _dot, _label }
        };
        Apply();
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Kind
    {
        get => (string)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    private static void OnChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((StatusPill)bindable).Apply();

    private void Apply()
    {
        var (ink, soft) = Kind switch
        {
            "success" => ("#34D399", "#1F34D399"),
            "danger" => ("#F87171", "#1FF87171"),
            "warning" => ("#FBBF24", "#1FFBBF24"),
            "accent" => ("#9D8CFF", "#249D8CFF"),
            _ => ("#8E8E93", "#1B1B1B")
        };
        var color = Color.FromArgb(ink);
        BackgroundColor = Color.FromArgb(soft);
        _dot.Fill = color;
        _label.Text = Text;
        _label.TextColor = color;
        IsVisible = !string.IsNullOrEmpty(Text);
    }
}
