using Microsoft.Maui.Controls.Shapes;

namespace UI.Controls;

public class SegmentedControl : Border
{
    public static readonly BindableProperty ModeProperty =
        BindableProperty.Create(nameof(Mode), typeof(string), typeof(SegmentedControl), "attendance", propertyChanged: Rebuild);

    public static readonly BindableProperty SelectedKeyProperty =
        BindableProperty.Create(nameof(SelectedKey), typeof(string), typeof(SegmentedControl), null, BindingMode.TwoWay, propertyChanged: OnSelectedChanged);

    private readonly HorizontalStackLayout _row = new() { Spacing = 4 };
    private bool _syncing;

    public SegmentedControl()
    {
        BackgroundColor = Color.FromArgb("#0A0A0A");
        Stroke = Color.FromArgb("#232323");
        StrokeThickness = 1;
        StrokeShape = new RoundRectangle { CornerRadius = 9 };
        Padding = new Thickness(3);
        Content = _row;
        Rebuild(this, null, Mode);
    }

    public string Mode
    {
        get => (string)GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public string? SelectedKey
    {
        get => (string?)GetValue(SelectedKeyProperty);
        set => SetValue(SelectedKeyProperty, value);
    }

    private static void Rebuild(BindableObject bindable, object? oldValue, object? newValue) =>
        ((SegmentedControl)bindable).Build();

    private static void OnSelectedChanged(BindableObject bindable, object? oldValue, object? newValue) =>
        ((SegmentedControl)bindable).Paint();

    private IReadOnlyList<(string Key, string Label, string Kind)> Options =>
        Mode == "submission"
            ? [("Entregó", "Entregó", "success"), ("No entregó", "No entregó", "danger")]
            : [("Presente", "Presente", "success"), ("Ausente", "Ausente", "danger")];

    private void Build()
    {
        _row.Children.Clear();
        foreach (var option in Options)
        {
            var button = new Button
            {
                Text = option.Label,
                FontFamily = "InterMedium",
                FontSize = 13,
                Padding = new Thickness(12, 5),
                CornerRadius = 6,
                BorderWidth = 0,
                MinimumHeightRequest = 0,
                MinimumWidthRequest = 0,
                BackgroundColor = Colors.Transparent
            };
            var key = option.Key;
            button.Clicked += (_, _) =>
            {
                if (SelectedKey == key)
                    return;
                SelectedKey = key;
            };
            _row.Add(button);
        }

        Paint();
    }

    private void Paint()
    {
        if (_syncing)
            return;

        _syncing = true;
        var index = 0;
        foreach (var option in Options)
        {
            if (_row.Children[index] is Button button)
            {
                var on = SelectedKey == option.Key;
                var color = option.Kind == "danger" ? Color.FromArgb("#F87171") : Color.FromArgb("#34D399");
                var soft = option.Kind == "danger" ? Color.FromArgb("#1FF87171") : Color.FromArgb("#1F34D399");
                button.BackgroundColor = on ? soft : Colors.Transparent;
                button.TextColor = on ? color : Color.FromArgb("#8E8E93");
            }

            index++;
        }

        _syncing = false;
    }
}
