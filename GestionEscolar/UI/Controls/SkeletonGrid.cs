namespace UI.Controls;

public class SkeletonGrid : VerticalStackLayout
{
    private const string PulseAnimationName = "SkeletonGridPulse";

    public static readonly BindableProperty RowCountProperty =
        BindableProperty.Create(nameof(RowCount), typeof(int), typeof(SkeletonGrid), 5,
            propertyChanged: (b, _, _) => ((SkeletonGrid)b).RebuildRows());

    public static readonly BindableProperty RowHeightProperty =
        BindableProperty.Create(nameof(RowHeight), typeof(double), typeof(SkeletonGrid), 60.0,
            propertyChanged: (b, _, _) => ((SkeletonGrid)b).RebuildRows());

    public static readonly BindableProperty RowTemplateProperty =
        BindableProperty.Create(nameof(RowTemplate), typeof(DataTemplate), typeof(SkeletonGrid), null,
            propertyChanged: (b, _, _) => ((SkeletonGrid)b).RebuildRows());

    public static readonly BindableProperty SeparatorColorProperty =
        BindableProperty.Create(nameof(SeparatorColor), typeof(Color), typeof(SkeletonGrid), Color.FromArgb("#232323"),
            propertyChanged: (b, _, _) => ((SkeletonGrid)b).RebuildRows());

    public SkeletonGrid()
    {
        Spacing = 0;
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    public int RowCount
    {
        get => (int)GetValue(RowCountProperty);
        set => SetValue(RowCountProperty, value);
    }

    public double RowHeight
    {
        get => (double)GetValue(RowHeightProperty);
        set => SetValue(RowHeightProperty, value);
    }

    public DataTemplate? RowTemplate
    {
        get => (DataTemplate?)GetValue(RowTemplateProperty);
        set => SetValue(RowTemplateProperty, value);
    }

    public Color SeparatorColor
    {
        get => (Color)GetValue(SeparatorColorProperty);
        set => SetValue(SeparatorColorProperty, value);
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == nameof(IsVisible))
        {
            if (IsVisible)
                StartPulse();
            else
                StopPulse();
        }
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        foreach (var child in Children)
        {
            if (child is BindableObject bindable)
                bindable.BindingContext = BindingContext;
        }
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        RebuildRows();
        StartPulse();
    }

    private void OnUnloaded(object? sender, EventArgs e)
    {
        StopPulse();
    }

    private void StartPulse()
    {
        StopPulse();
        if (!IsVisible || Window is null)
            return;

        var pulse = new Animation
        {
            { 0.0, 0.5, new Animation(v => Opacity = v, 0.40, 0.95, Easing.SinInOut) },
            { 0.5, 1.0, new Animation(v => Opacity = v, 0.95, 0.40, Easing.SinInOut) }
        };
        pulse.Commit(this, PulseAnimationName, length: 1200, repeat: () => IsVisible && Window is not null);
    }

    private void StopPulse()
    {
        this.AbortAnimation(PulseAnimationName);
        Opacity = 1.0;
    }

    private void RebuildRows()
    {
        Children.Clear();
        if (RowCount <= 0)
            return;

        var template = RowTemplate ?? CreateDefaultRowTemplate();

        for (int i = 0; i < RowCount; i++)
        {
            var content = template.CreateContent();
            if (content is View view)
            {
                if (view.HeightRequest <= 0 && RowHeight > 0)
                    view.HeightRequest = RowHeight;

                view.BindingContext = BindingContext;
                Children.Add(view);

                if (i < RowCount - 1)
                {
                    Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        Color = SeparatorColor
                    });
                }
            }
        }
    }

    private static DataTemplate CreateDefaultRowTemplate()
    {
        return new DataTemplate(() =>
        {
            var grid = new Grid
            {
                Padding = new Thickness(20, 0),
                ColumnDefinitions = new ColumnDefinitionCollection
                {
                    new ColumnDefinition(GridLength.Auto),
                    new ColumnDefinition(GridLength.Star)
                },
                ColumnSpacing = 12,
                HeightRequest = 60
            };

            var avatar = new SkeletonBlock
            {
                WidthRequest = 32,
                HeightRequest = 32,
                IsCircle = true,
                VerticalOptions = LayoutOptions.Center
            };

            var text = new SkeletonBlock
            {
                WidthRequest = 180,
                HeightRequest = 14,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Start
            };

            Grid.SetColumn(avatar, 0);
            Grid.SetColumn(text, 1);

            grid.Children.Add(avatar);
            grid.Children.Add(text);

            return grid;
        });
    }
}

