using LCalc;
using LCalc.Cli;
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
            AnsiConsole.MarkupLine("[red]This program cannot be run in non-interactive mode[/]");
            return;
        }

        AnsiConsole.MarkupLine("Press [Yellow]Ctrl-C[/] to exit");

        Cli.RunLoop();
    }
}