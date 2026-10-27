namespace UI.Controls;

public static class IconPaths
{
    public static string? For(string? name) => name switch
    {
        "users" => "M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2 M13 7a4 4 0 1 1-8 0 4 4 0 0 1 8 0 M22 21v-2a4 4 0 0 0-3-3.87 M16 3.13a4 4 0 0 1 0 7.75",
        "activity" => "M22 12h-4l-3 9L9 3l-3 9H2",
        "inbox" => "M22 12h-6l-2 3h-4l-2-3H2 M5.45 5.11 2 12v6a2 2 0 0 0 2 2h16a2 2 0 0 0 2-2v-6l-3.45-6.89A2 2 0 0 0 16.76 4H7.24a2 2 0 0 0-1.79 1.11z",
        "check" => "M22 11.08V12a10 10 0 1 1-5.93-9.14 M9 11l3 3L22 4",
        "calendar" => "M19 4H5a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2z M16 2v4 M8 2v4 M3 10h18",
        "sliders" => "M4 21v-7 M4 10V3 M12 21v-9 M12 8V3 M20 21v-5 M20 12V3 M1 14h6 M9 8h6 M17 16h6",
        "search" => "M11 3a8 8 0 1 0 0 16a8 8 0 1 0 0-16 M21 21l-4.3-4.3",
        "chevron-left" => "M15 18l-6-6 6-6",
        "chevron-down" => "M6 9l6 6 6-6",
        _ => null
    };
}
