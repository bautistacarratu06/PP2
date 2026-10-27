using UI.Formatting;

namespace UI.Controls;

public partial class Avatar : ContentView
{
    public static readonly BindableProperty FullNameProperty =
        BindableProperty.Create(nameof(FullName), typeof(string), typeof(Avatar), string.Empty, propertyChanged: OnNameChanged);

    public static readonly BindableProperty SizeProperty =
        BindableProperty.Create(nameof(Size), typeof(double), typeof(Avatar), 34d, propertyChanged: OnSizeChanged);

    public Avatar()
    {
        InitializeComponent();
    }

    public string FullName
    {
        get => (string)GetValue(FullNameProperty);
        set => SetValue(FullNameProperty, value);
    }

    public double Size
    {
        get => (double)GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    private static void OnNameChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (Avatar)bindable;
        view.InitialsLabel.Text = PersonName.Initials(view.FullName);
    }

    private static void OnSizeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (Avatar)bindable;
        var size = (double)newValue;
        view.Circle.WidthRequest = size;
        view.Circle.HeightRequest = size;
        view.InitialsLabel.FontSize = Math.Max(11, size * 0.35);
    }
}
