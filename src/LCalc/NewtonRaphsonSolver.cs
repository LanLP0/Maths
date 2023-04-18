using Common.Results;

namespace LCalc;

public delegate Result<double> Function(double unknown);

internal static class NewtonRaphsonSolver
{
    private const double Epsilon = 1E-10;
    
    /// <summary>
    /// The approximate of derivative of func()
    /// </summary>
    private static Result<double> Derivative(Function f, double fx, double x, double h)
    {
        var fxh = f(x + h);
        if (fxh.Faulted)
            return fxh;
        
        return (fxh.Value - fx) / h;
    }

    /// <summary>
    /// 
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
                return fx;
            
            if (Math.Abs(fx.Value) < Epsilon) // If we have found the approx root
                break;
            
            var dfx = Derivative(f, fx.Value, x0, 1E-5);

            // dfx cannot be 0
            if (Math.Abs(dfx.Value) < Epsilon)
                return new DfxZeroException("Cannot solve"
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
        if (x.Faulted && x.Exception! is not DfxZeroException)
            return x;

        var y1 = f(x.Value);

        if (y1.Faulted)
            return Err("Cannot solve"
#if DEBUG
                        + ' ' + y1.Exception!.Message
#endif
            );
        
        if (Math.Abs(y1.Value) < Epsilon)
            return x;

        x = Solve(f, -1);
        if (x.Faulted && x.Exception! is not DfxZeroException)
            return x;

        var y2 = f(x.Value);

        if (y2.Faulted)
            return Err("Cannot solve"
#if DEBUG
                       + ' ' + y2.Exception!.Message
#endif
            );
        
        if (Math.Abs(y2.Value) < Epsilon)
            return x;

        x = Solve(f, 1);
        if (x.Faulted && x.Exception! is not DfxZeroException)
            return x;

        var y3 = f(x.Value);

        if (y3.Faulted)
            return Err("Cannot solve"
#if DEBUG
                       + ' ' + y3.Exception!.Message
#endif
            );
        
        if (Math.Abs(y3.Value) < Epsilon)
            return x;

        return Err("Cannot solve"
#if DEBUG
                   + " (3 tries ended)"
#endif
        );
    }
    
    private sealed class DfxZeroException : Exception
    {
        public override string Message { get; }

        public DfxZeroException(string message)
        {
            Message = message;
        }
    }
}