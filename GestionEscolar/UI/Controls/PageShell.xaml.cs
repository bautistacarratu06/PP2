using System.Windows.Input;

namespace UI.Controls;

public partial class PageShell : ContentView
{
    public static readonly BindableProperty TitleProperty =
        BindableProperty.Create(nameof(Title), typeof(string), typeof(PageShell), string.Empty);

    public static readonly BindableProperty SubtitleProperty =
        BindableProperty.Create(nameof(Subtitle), typeof(string), typeof(PageShell), string.Empty);

    public static readonly BindableProperty ShowBackProperty =
        BindableProperty.Create(nameof(ShowBack), typeof(bool), typeof(PageShell), false);

    public static readonly BindableProperty BackTextProperty =
        BindableProperty.Create(nameof(BackText), typeof(string), typeof(PageShell), "Seguimiento");

    public static readonly BindableProperty BackCommandProperty =
        BindableProperty.Create(nameof(BackCommand), typeof(ICommand), typeof(PageShell));

    public static readonly BindableProperty ShellPaddingProperty =
        BindableProperty.Create(nameof(ShellPadding), typeof(Thickness), typeof(PageShell), new Thickness(40, 36, 40, 56));

    public static readonly BindableProperty ContentWidthProperty =
        BindableProperty.Create(nameof(ContentWidth), typeof(double), typeof(PageShell), 1160d);

    private Grid? _header;
    private SectionPicker? _picker;

    public PageShell()
    {
        InitializeComponent();
        SizeChanged += (_, _) => Fit();
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _header = GetTemplateChild("Header") as Grid;
        _picker = GetTemplateChild("Picker") as SectionPicker;
        Fit();
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Subtitle
    {
        get => (string)GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public bool ShowBack
    {
        get => (bool)GetValue(ShowBackProperty);
        set => SetValue(ShowBackProperty, value);
    }

    public string BackText
    {
        get => (string)GetValue(BackTextProperty);
        set => SetValue(BackTextProperty, value);
    }

    public ICommand? BackCommand
    {
        get => (ICommand?)GetValue(BackCommandProperty);
        set => SetValue(BackCommandProperty, value);
    }

    public Thickness ShellPadding
    {
        get => (Thickness)GetValue(ShellPaddingProperty);
        set => SetValue(ShellPaddingProperty, value);
    }

    public double ContentWidth
    {
        get => (double)GetValue(ContentWidthProperty);
        set => SetValue(ContentWidthProperty, value);
    }

    private void Fit()
    {
        if (double.IsNaN(Width) || Width <= 0)
            return;

        var narrow = Width < 760;
        ShellPadding = narrow ? new Thickness(18, 24, 18, 40) : new Thickness(40, 36, 40, 56);
        ContentWidth = Math.Min(1160, Width - (narrow ? 36 : 80));
        if (_header is null || _picker is null)
            return;

        _header.ColumnDefinitions =
        [
            new ColumnDefinition(GridLength.Star),
            new ColumnDefinition(narrow ? new GridLength(0) : GridLength.Auto)
        ];
        _header.RowDefinitions = narrow
            ? [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)]
            : [new RowDefinition(GridLength.Auto)];
        _header.RowSpacing = narrow ? 16 : 0;
        Grid.SetColumn(_picker, narrow ? 0 : 1);
        Grid.SetRow(_picker, narrow ? 1 : 0);
        _picker.HorizontalOptions = narrow ? LayoutOptions.Start : LayoutOptions.End;
    }
}
