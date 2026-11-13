namespace UI.Controls;

public class HoverRow : Grid
{
    public HoverRow()
    {
        var pointer = new PointerGestureRecognizer();
        pointer.PointerEntered += (_, _) =>
        {
            BackgroundColor = Color.FromArgb("#1B1B1B");
            VisualStateManager.GoToState(this, "PointerOver");
        };
        pointer.PointerExited += (_, _) =>
        {
            BackgroundColor = Colors.Transparent;
            VisualStateManager.GoToState(this, "Normal");
        };
        GestureRecognizers.Add(pointer);
    }
}
