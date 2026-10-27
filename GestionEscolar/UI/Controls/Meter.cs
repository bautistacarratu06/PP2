using Microsoft.Maui.Controls.Shapes;

namespace UI.Controls;

public class Meter : Grid
{
    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(nameof(Value), typeof(double), typeof(Meter), 0d, propertyChanged: OnChanged);

    public static readonly BindableProperty FillProperty =
        BindableProperty.Create(nameof(Fill), typeof(Color), typeof(Meter), null, propertyChanged: OnChanged);

    public static readonly BindableProperty BarWidthProperty =
        BindableProperty.Create(nameof(BarWidth), typeof(double), typeof(Meter), 110d, propertyChanged: OnChanged);

    public static readonly BindableProperty ShowLabelProperty =
        BindableProperty.Create(nameof(ShowLabel), typeof(bool), typeof(Meter), true, propertyChanged: OnChanged);

    public static readonly BindableProperty BarHeightProperty =
        BindableProperty.Create(nameof(BarHeight), typeof(double), typeof(Meter), 5d, propertyChanged: OnChanged);

    private readonly Border _track;
    private readonly Border _fill;
    private readonly Label _label;

    public Meter()
    {
        ColumnDefinitions = new ColumnDefinitionCollection
        {
            new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(GridLength.Auto)
        };
        ColumnSpacing = 10;
        _track = new Border
        {
            BackgroundColor = Color.FromArgb("#1B1B1B"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 3 },
            HorizontalOptions = LayoutOptions.Fill,
            VerticalOptions = LayoutOptions.Center
        };
        _fill = new Border
        {
            BackgroundColor = Color.FromArgb("#8E8E93"),
            StrokeThickness = 0,
            StrokeShape = new RoundRectangle { CornerRadius = 3 },
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Fill
        };
        _track.Content = _fill;
        _label = new Label
        {
            FontFamily = "Inter",
            FontSize = 13,
            TextColor = Color.FromArgb("#8E8E93"),
            WidthRequest = 34,
            VerticalOptions = LayoutOptions.Center
        };
        Add(_track);
        Add(_label);
        Grid.SetColumn(_label, 1);
        Apply();
    }

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Color? Fill
    {
        get => (Color?)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    public double BarWidth
    {
        get => (double)GetValue(BarWidthProperty);
        set => SetValue(BarWidthProperty, value);
    }

    public bool ShowLabel
    {
        get => (bool)GetValue(ShowLabelProperty);
        set => SetValue(ShowLabelProperty, value);
    }

    public double BarHeight
    {
        get => (double)GetValue(BarHeightProperty);
        set => SetValue(BarHeightProperty, value);
    }

    private static void OnChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((Meter)bindable).Apply();

    private void Apply()
    {
        var ratio = Math.Clamp(Value, 0, 1);
        _track.HeightRequest = BarHeight;
        _track.MaximumWidthRequest = BarWidth;
        _fill.WidthRequest = Math.Max(0, BarWidth * ratio);
        _fill.BackgroundColor = Fill ?? Color.FromArgb("#8E8E93");
        _label.IsVisible = ShowLabel;
        _label.Text = $"{Math.Round(ratio * 100)}%";
    }
}
