using Common.Results;

namespace LCalc;

internal delegate Result<double> Function(double unknown);

internal static class NewtonRaphsonSolver
{
    private const double Epsilon = 1E-10;

    /// <summary>
    ///     The approximate of derivative of func()
    /// </summary>
    private static Result<double> Derivative(Function f, double fx, double x, double h)
    {
        var fxh = f(x + h);
        if (fxh.Faulted)
            return fxh;

        return (fxh.Value - fx) / h;
    }

    /// <summary>
    /// </summary>
    /// <param name="f">The function</param>
    /// <param name="x0">Initial guess</param>
    /// <param name="maxIteration">The max number of iteration(s)</param>
    public static Result<double> Solve(Function f, double x0, int maxIteration = 20)
    {
        for (var iteration = 1; iteration <= maxIteration; iteration++)
        {
            var fx = f(x0);
            if (fx.Faulted)
                return Err<double>("Cannot solve"
#if DEBUG
                    + " (f(x) failed)"
#endif
                );

            if (Math.Abs(fx.Value) < Epsilon) // If we have found the approx root
                break;

            var dfx = Derivative(f, fx.Value, x0, 1E-5);
            if (dfx.Faulted)
                return Err<double>("Cannot solve"
#if DEBUG
                    + " (f'(x) failed)"
#endif
                );

            // dfx cannot be 0
            if (Math.Abs(dfx.Value) < Epsilon)
                return Err("Cannot solve"
#if DEBUG
                    + " (dfx = 0)"
#endif
                );

            x0 -= fx.Value / dfx.Value;
        }

        return x0;
    }

    public static Result<double> SolveFor(MathTree.MathTree tree, string unknown)
    {
        var f = new Function(x =>
        {
            tree.Scope.Variables.OverrideAdd(unknown, x);
            var result = tree.Calc();
            if (result.Faulted)
                return result.Exception!;

            return result.Number!.Value;
        });

        var x = Solve(f, 0);
        if (!x.Faulted)
        {
            var y = f(x.Value);

            if (y.Faulted)
                return Err("Cannot solve"
#if DEBUG
                    + ' ' + y.Exception!.Message
#endif
                );

            if (Math.Abs(y.Value) < Epsilon)
                return x;
        }

        for (var x0 = 2187; x0 >= 1; x0 /= 3) // powers of 3: 1, 3, 9, .., 2187
        {
            Result<double> y;
            x = Solve(f, x0);
            if (x.Success)
            {
                y = f(x.Value);

                if (y.Success && Math.Abs(y.Value) < Epsilon)
                    return x;
            }

            x = Solve(f, -x0);
            if (x.Faulted)
                continue;

            y = f(x.Value);

            if (y.Faulted)
                continue;

            if (Math.Abs(y.Value) < Epsilon)
                return x;
        }

        return Err("Cannot solve"
#if DEBUG
            + " (15 tries ended)"
#endif
        );
    }
}