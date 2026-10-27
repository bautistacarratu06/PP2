namespace UI.Controls;

public partial class FilterChips : ContentView
{
    public static readonly BindableProperty SelectedKeyProperty =
        BindableProperty.Create(nameof(SelectedKey), typeof(string), typeof(FilterChips), "Todos", BindingMode.TwoWay, propertyChanged: OnChanged);

    public FilterChips()
    {
        InitializeComponent();
        Paint();
    }

    public string SelectedKey
    {
        get => (string)GetValue(SelectedKeyProperty);
        set => SetValue(SelectedKeyProperty, value);
    }

    private static void OnChanged(BindableObject bindable, object oldValue, object newValue) =>
        ((FilterChips)bindable).Paint();

    private void OnChip(object? sender, EventArgs e)
    {
        if (sender is Button button && button.Text is not null)
            SelectedKey = button.Text;
    }

    private void Paint()
    {
        if (Content is not Border { Content: HorizontalStackLayout row })
            return;

        foreach (var child in row.Children)
        {
            if (child is not Button button)
                continue;

            var on = button.Text == SelectedKey;
            button.BackgroundColor = on ? Color.FromArgb("#1B1B1B") : Colors.Transparent;
            button.TextColor = on ? Color.FromArgb("#EDEDED") : Color.FromArgb("#8E8E93");
            button.BorderWidth = 0;
        }
    }
}
