using Microsoft.Maui.Controls.Shapes;

namespace UI.Controls;

public class Icon : ContentView
{
    public static readonly BindableProperty NameProperty =
        BindableProperty.Create(nameof(Name), typeof(string), typeof(Icon), null, propertyChanged: OnChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(Icon), 17d, propertyChanged: OnChanged);

    public static readonly BindableProperty GlyphColorProperty =
        BindableProperty.Create(nameof(GlyphColor), typeof(Color), typeof(Icon), null, propertyChanged: OnChanged);

    private readonly Grid _glyphs = new();

    public Icon()
    {
        Content = _glyphs;
        Apply();
    }

    public string? Name
    {
        get => (string?)GetValue(NameProperty);
        set => SetValue(NameProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    public Color? GlyphColor
    {
        get => (Color?)GetValue(GlyphColorProperty);
        set => SetValue(GlyphColorProperty, value);
    }

    private static void OnChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((Icon)bindable).Apply();

    private void Apply()
    {
        WidthRequest = Size;
        HeightRequest = Size;
        _glyphs.WidthRequest = Size;
        _glyphs.HeightRequest = Size;
        _glyphs.Children.Clear();

        var data = IconPaths.For(Name);
        if (string.IsNullOrEmpty(data))
            return;

        // Win2D cierra la app si un Path tiene más de una figura. Cada trazo va en el suyo,
        // escalado desde el viewBox 24 para que sigan alineados.
        var scale = Size / 24d;
        var stroke = GlyphColor ?? Color.FromArgb("#8E8E93");
        foreach (var figure in Figures(data))
        {
            _glyphs.Add(new Microsoft.Maui.Controls.Shapes.Path
            {
                Data = (Geometry?)new PathGeometryConverter().ConvertFromInvariantString(figure),
                Aspect = Stretch.None,
                Stroke = stroke,
                StrokeLineCap = PenLineCap.Round,
                StrokeLineJoin = PenLineJoin.Round,
                StrokeThickness = 1.7,
                Scale = scale,
                AnchorX = 0,
                AnchorY = 0,
                HorizontalOptions = LayoutOptions.Start,
                VerticalOptions = LayoutOptions.Start
            });
        }
    }

    private static IEnumerable<string> Figures(string data)
    {
        foreach (var part in data.Split(" M"))
        {
            var figure = part.Trim();
            if (figure.Length == 0)
                continue;

            yield return figure.StartsWith('M') ? figure : "M" + figure;
        }
    }
}
