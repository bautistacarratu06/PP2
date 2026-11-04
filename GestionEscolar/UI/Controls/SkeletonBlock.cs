using Microsoft.Maui.Controls.Shapes;

namespace UI.Controls;

public class SkeletonBlock : Border
{
    public static readonly BindableProperty IsCircleProperty =
        BindableProperty.Create(nameof(IsCircle), typeof(bool), typeof(SkeletonBlock), false,
            propertyChanged: (bindable, _, _) => ((SkeletonBlock)bindable).UpdateShape());

    public static readonly BindableProperty RadiusProperty =
        BindableProperty.Create(nameof(Radius), typeof(double), typeof(SkeletonBlock), 6.0,
            propertyChanged: (bindable, _, _) => ((SkeletonBlock)bindable).UpdateShape());

    public SkeletonBlock()
    {
        BackgroundColor = Color.FromArgb("#242424");
        Stroke = Colors.Transparent;
        StrokeThickness = 0;
        UpdateShape();
    }

    public bool IsCircle
    {
        get => (bool)GetValue(IsCircleProperty);
        set => SetValue(IsCircleProperty, value);
    }

    public double Radius
    {
        get => (double)GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(HeightRequest) || propertyName == nameof(WidthRequest))
        {
            if (IsCircle)
                UpdateShape();
        }
    }

    private void UpdateShape()
    {
        double cornerRadius = Radius;
        if (IsCircle)
        {
            var dimension = HeightRequest > 0 ? HeightRequest : (WidthRequest > 0 ? WidthRequest : 32);
            cornerRadius = dimension / 2.0;
        }

        StrokeShape = new RoundRectangle
        {
            CornerRadius = new CornerRadius(cornerRadius)
        };
    }
}

