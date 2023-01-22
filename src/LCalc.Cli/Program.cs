using LCalc;

// using org.mariuszgromada.math.mxparser;

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
                input = Console.ReadLine()!;
                Console.WriteLine(Calculator.CalcFormatted(input));
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                Console.ForegroundColor = ConsoleColor.Gray;
            }

        // for(;;)
        // {
        //     try
        //     {
        //         Console.Write("Expression: ");
        //         var input = Console.ReadLine()!;
        //         var exp = new Expression(input);
        //         Console.WriteLine(exp.calculate());
        //     }
        //     catch (Exception e)
        //     {
        //         Console.ForegroundColor = ConsoleColor.Red;
        //         Console.WriteLine(e);
        //         Console.ForegroundColor = ConsoleColor.Gray;
        //     }
        // }
    }
}