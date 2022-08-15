namespace Common.Cli;

public static class EnumHelpers
{
    public static bool TryParseFast(string text, out ConsoleColor consoleColor)
    {
        ConsoleColor? color = text switch
        {
            nameof(ConsoleColor.Black) => ConsoleColor.Black,
            nameof(ConsoleColor.DarkBlue) => ConsoleColor.DarkBlue,
            nameof(ConsoleColor.DarkGreen) => ConsoleColor.DarkGreen,
            nameof(ConsoleColor.DarkCyan) => ConsoleColor.DarkCyan,
            nameof(ConsoleColor.DarkRed) => ConsoleColor.DarkRed,
            nameof(ConsoleColor.DarkMagenta) => ConsoleColor.DarkMagenta,
            nameof(ConsoleColor.DarkYellow) => ConsoleColor.DarkYellow,
            nameof(ConsoleColor.Gray) => ConsoleColor.Gray,
            nameof(ConsoleColor.DarkGray) => ConsoleColor.DarkGray,
            nameof(ConsoleColor.Blue) => ConsoleColor.Blue,
            nameof(ConsoleColor.Green) => ConsoleColor.Green,
            nameof(ConsoleColor.Cyan) => ConsoleColor.Cyan,
            nameof(ConsoleColor.Red) => ConsoleColor.Red,
            nameof(ConsoleColor.Magenta) => ConsoleColor.Magenta,
            nameof(ConsoleColor.Yellow) => ConsoleColor.Yellow,
            nameof(ConsoleColor.White) => ConsoleColor.White,
            _ => null
        };

        if (!color.HasValue)
        {
            consoleColor = default;
            return false;
        }

        consoleColor = color.Value;
        return true;
    }

    public static string FastConsoleKeyToNumberString(ConsoleKey value)
    {
        return value switch
        {
            ConsoleKey.D0 => "0",
            ConsoleKey.D1 => "1",
            ConsoleKey.D2 => "2",
            ConsoleKey.D3 => "3",
            ConsoleKey.D4 => "4",
            ConsoleKey.D5 => "5",
            ConsoleKey.D6 => "6",
            ConsoleKey.D7 => "7",
            ConsoleKey.D8 => "8",
            ConsoleKey.D9 => "9",
            ConsoleKey.NumPad0 => "0",
            ConsoleKey.NumPad1 => "1",
            ConsoleKey.NumPad2 => "2",
            ConsoleKey.NumPad3 => "3",
            ConsoleKey.NumPad4 => "4",
            ConsoleKey.NumPad5 => "5",
            ConsoleKey.NumPad6 => "6",
            ConsoleKey.NumPad7 => "7",
            ConsoleKey.NumPad8 => "8",
            ConsoleKey.NumPad9 => "9",
            _ => "0"
        };
    }
}