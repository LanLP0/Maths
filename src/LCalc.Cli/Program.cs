using LCalc;

internal sealed class Program
{
    public static void Main(string[] args)
    {
        Console.CancelKeyPress += (_, _) => Environment.Exit(0);

        var input = string.Join(' ', args);
        if (args.Length is not 0)
        {
            Console.WriteLine(Calculator.CalcFormatted(input));
            return;
        }

        Console.WriteLine("Press ^C to exit");

        for (;;)
            try
            {
                Console.Write("Expression: ");
                input = Console.ReadLine();
                if (input is null)
                    break;

                if (string.IsNullOrWhiteSpace(input))
                    break;

                Console.WriteLine(Calculator.CalcFormatted(input));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.Gray;
            }
    }
}