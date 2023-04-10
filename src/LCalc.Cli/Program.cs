using LCalc;
using Spectre.Console;

internal sealed class Program
{
    public static void Main(string[] args)
    {
        Console.CancelKeyPress += (_, _) => Environment.Exit(0);

        if (args.Length is not 0)
        {
            var input = string.Join(' ', args);
            var result = Calculator.CalcFormatted(input);
            AnsiConsole.WriteLine(result);
            return;
        }

        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.MarkupLine(
                "[red]This program needs to be run in interactive mode when run with no arguments[/]");

            return;
        }

        AnsiConsole.MarkupLine("Press [Yellow]Ctrl-C[/] to exit");

        for (;;)
            try
            {
                var input = AnsiConsole.Ask<string>("Expression: ");
                if (string.IsNullOrWhiteSpace(input))
                    break;

                var result = Calculator.CalcFormatted(input);
                AnsiConsole.WriteLine(result);
            }
            catch (Exception e)
            {
                AnsiConsole.WriteException(e);
            }
    }
}