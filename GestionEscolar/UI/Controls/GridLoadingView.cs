namespace UI.Controls;

[ContentProperty(nameof(LoadedView))]
public class GridLoadingView : ContentView
{
    private readonly Grid _container = new();
    private readonly Label _defaultErrorLabel = new()
    {
        TextColor = Color.FromArgb("#F87171"),
        Padding = new Thickness(20, 12),
        FontFamily = "InterMedium",
        FontSize = 13,
        IsVisible = false
    };

    private SkeletonGrid? _defaultSkeletonGrid;
    private bool _isStateExplicitlySet;

    public static readonly BindableProperty StateProperty =
        BindableProperty.Create(nameof(State), typeof(GridLoadingState), typeof(GridLoadingView), GridLoadingState.Loaded,
            propertyChanged: (b, _, _) =>
            {
                var view = (GridLoadingView)b;
                view._isStateExplicitlySet = true;
                view.UpdateVisibility();
            });

    public static readonly BindableProperty IsLoadingProperty =
        BindableProperty.Create(nameof(IsLoading), typeof(bool), typeof(GridLoadingView), false,
            propertyChanged: (b, _, _) => ((GridLoadingView)b).UpdateVisibility());

    public static readonly BindableProperty HasErrorProperty =
        BindableProperty.Create(nameof(HasError), typeof(bool), typeof(GridLoadingView), false,
            propertyChanged: (b, _, _) => ((GridLoadingView)b).UpdateVisibility());

    public static readonly BindableProperty ErrorMessageProperty =
        BindableProperty.Create(nameof(ErrorMessage), typeof(string), typeof(GridLoadingView), string.Empty,
            propertyChanged: (b, _, n) =>
            {
                var view = (GridLoadingView)b;
                view._defaultErrorLabel.Text = (string?)n ?? string.Empty;
                view.UpdateVisibility();
            });

    public static readonly BindableProperty SkeletonViewProperty =
        BindableProperty.Create(nameof(SkeletonView), typeof(View), typeof(GridLoadingView), null,
            propertyChanged: (b, o, n) => ((GridLoadingView)b).OnSkeletonViewChanged((View?)o, (View?)n));

    public static readonly BindableProperty LoadedViewProperty =
        BindableProperty.Create(nameof(LoadedView), typeof(View), typeof(GridLoadingView), null,
            propertyChanged: (b, o, n) => ((GridLoadingView)b).OnLoadedViewChanged((View?)o, (View?)n));

    public static readonly BindableProperty ErrorViewProperty =
        BindableProperty.Create(nameof(ErrorView), typeof(View), typeof(GridLoadingView), null,
            propertyChanged: (b, o, n) => ((GridLoadingView)b).OnErrorViewChanged((View?)o, (View?)n));

    public GridLoadingView()
    {
        _container.Children.Add(_defaultErrorLabel);
        base.Content = _container;
        UpdateVisibility();
    }

    public GridLoadingState State
    {
        get => (GridLoadingState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    public bool IsLoading
    {
        get => (bool)GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    public bool HasError
    {
        get => (bool)GetValue(HasErrorProperty);
        set => SetValue(HasErrorProperty, value);
    }

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public View? SkeletonView
    {
        get => (View?)GetValue(SkeletonViewProperty);
        set => SetValue(SkeletonViewProperty, value);
    }

    public View? LoadedView
    {
        get => (View?)GetValue(LoadedViewProperty);
        set => SetValue(LoadedViewProperty, value);
    }

    public View? ErrorView
    {
        get => (View?)GetValue(ErrorViewProperty);
        set => SetValue(ErrorViewProperty, value);
    }

    private void OnSkeletonViewChanged(View? oldView, View? newView)
    {
        if (oldView != null && _container.Children.Contains(oldView))
            _container.Children.Remove(oldView);

        if (_defaultSkeletonGrid != null && _container.Children.Contains(_defaultSkeletonGrid))
            _container.Children.Remove(_defaultSkeletonGrid);

        if (newView != null)
            _container.Children.Add(newView);

        UpdateVisibility();
    }

    private void OnLoadedViewChanged(View? oldView, View? newView)
    {
        if (oldView != null && _container.Children.Contains(oldView))
            _container.Children.Remove(oldView);

        if (newView != null)
            _container.Children.Add(newView);

        UpdateVisibility();
    }

    private void OnErrorViewChanged(View? oldView, View? newView)
    {
        if (oldView != null && _container.Children.Contains(oldView))
            _container.Children.Remove(oldView);

        if (newView != null)
            _container.Children.Add(newView);

        UpdateVisibility();
    }

    private GridLoadingState ResolveState()
    {
        if (_isStateExplicitlySet)
            return State;
        if (IsLoading)
            return GridLoadingState.Loading;
        if (HasError)
            return GridLoadingState.Error;
        return GridLoadingState.Loaded;
    }

    private void UpdateVisibility()
    {
        var activeState = ResolveState();

        // Si no se proveyó SkeletonView personalizado, creamos un SkeletonGrid por defecto
        if (SkeletonView == null && _defaultSkeletonGrid == null)
        {
            _defaultSkeletonGrid = new SkeletonGrid();
            _container.Children.Add(_defaultSkeletonGrid);
        }

        var skeleton = SkeletonView ?? (View?)_defaultSkeletonGrid;
        if (skeleton != null)
            skeleton.IsVisible = (activeState == GridLoadingState.Loading);

        if (LoadedView != null)
            LoadedView.IsVisible = (activeState == GridLoadingState.Loaded);

        if (ErrorView != null)
        {
            ErrorView.IsVisible = (activeState == GridLoadingState.Error);
            _defaultErrorLabel.IsVisible = false;
        }
        else
        {
            _defaultErrorLabel.IsVisible = (activeState == GridLoadingState.Error);
        }
    }
}

