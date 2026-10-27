using Microsoft.Maui.Controls.Shapes;

namespace UI.Controls;

public partial class SidebarView : ContentView
{
    private static readonly (string Title, string Route, string Icon)[] Items =
    [
        ("Nómina", "StudentListPage", "users"),
        ("Seguimiento", "TrackingPage", "activity"),
        ("Entregas", "Submissions", "inbox"),
        ("Asistencia", "Attendance", "check"),
        ("Calendario", "Calendar", "calendar"),
        ("Ajustes", "SettingsPage", "sliders")
    ];

    private readonly Dictionary<string, Border> _rows = new();
    private bool _collapsed;
    private bool _hooked;

    public SidebarView()
    {
        InitializeComponent();
        Build();
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        if (_hooked || Shell.Current is not { } shell)
            return;

        _hooked = true;
        shell.Navigated += (_, _) => MainThread.BeginInvokeOnMainThread(Paint);
        var width = Window?.Width ?? shell.Width;
        if (width is > 0 and < 860)
            SetCollapsed(true, animate: false);
        Paint();
    }

    private void Build()
    {
        foreach (var item in Items)
        {
            var icon = new Icon { Name = item.Icon, Size = 17, VerticalOptions = LayoutOptions.Center };
            var label = new Label
            {
                Text = item.Title,
                FontFamily = "InterMedium",
                FontSize = 13,
                TextColor = Color.FromArgb("#8E8E93"),
                VerticalOptions = LayoutOptions.Center
            };
            var mark = new BoxView
            {
                WidthRequest = 3,
                Color = Colors.Transparent,
                VerticalOptions = LayoutOptions.Fill,
                Margin = new Thickness(0, 8)
            };
            var body = new HorizontalStackLayout
            {
                Spacing = 12,
                Padding = new Thickness(10, 9, 10, 9),
                Children = { icon, label }
            };
            var row = new Border
            {
                BackgroundColor = Colors.Transparent,
                StrokeThickness = 0,
                StrokeShape = new RoundRectangle { CornerRadius = 8 },
                Padding = 0,
                Content = new Grid
                {
                    ColumnDefinitions =
                    {
                        new ColumnDefinition(3),
                        new ColumnDefinition(GridLength.Star)
                    },
                    Children = { mark, body }
                }
            };
            Grid.SetColumn(body, 1);
            ToolTipProperties.SetText(row, item.Title);
            var route = item.Route;
            var tap = new TapGestureRecognizer();
            tap.Tapped += async (_, _) =>
            {
                if (Shell.Current is null)
                    return;
                await Shell.Current.GoToAsync($"//{route}");
            };
            row.GestureRecognizers.Add(tap);
            row.BindingContext = (icon, label, mark);
            _rows[route] = row;
            Menu.Add(row);
        }
    }

    private void Paint()
    {
        var location = Shell.Current?.CurrentState?.Location.OriginalString ?? string.Empty;
        var active = location.Contains("StudentDetailPage", StringComparison.Ordinal)
            ? "TrackingPage"
            : Items.Select(item => item.Route).LastOrDefault(route => location.Contains(route, StringComparison.Ordinal))
              ?? "StudentListPage";

        foreach (var item in Items)
        {
            if (_rows[item.Route].BindingContext is not (Icon icon, Label label, BoxView mark))
                continue;

            var on = item.Route == active;
            _rows[item.Route].BackgroundColor = on ? Color.FromArgb("#1B1B1B") : Colors.Transparent;
            mark.Color = on ? Color.FromArgb("#9D8CFF") : Colors.Transparent;
            label.TextColor = on ? Color.FromArgb("#EDEDED") : Color.FromArgb("#8E8E93");
            label.IsVisible = !_collapsed;
            icon.GlyphColor = on ? Color.FromArgb("#9D8CFF") : Color.FromArgb("#8E8E93");
            if (_rows[item.Route].Content is Grid grid
                && grid.Children.Count > 1
                && grid.Children[1] is HorizontalStackLayout stack)
                stack.HorizontalOptions = _collapsed ? LayoutOptions.Center : LayoutOptions.Fill;
        }

        BrandText.IsVisible = !_collapsed;
        CollapseLabel.IsVisible = !_collapsed;
        Brand.HorizontalOptions = _collapsed ? LayoutOptions.Center : LayoutOptions.Fill;
        CollapseIcon.Rotation = _collapsed ? 180 : 0;
    }

    private void OnToggle(object? sender, TappedEventArgs e) => SetCollapsed(!_collapsed, animate: true);

    private void SetCollapsed(bool collapsed, bool animate)
    {
        _collapsed = collapsed;
        Paint();
        var shell = Shell.Current;
        if (shell is null)
            return;

        var target = collapsed ? 64 : 232;
        if (!animate)
        {
            shell.FlyoutWidth = target;
            return;
        }

        var from = shell.FlyoutWidth;
        var animation = new Animation(value => shell.FlyoutWidth = value, from, target);
        animation.Commit(this, "FlyoutWidth", 16, 200, Easing.CubicOut);
    }
}
