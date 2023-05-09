namespace MichinoekiTSP.Data.Solvers;

public class TwoOptSA : ITSPExecuter
{
    private readonly TSPSolverContext context;

    private readonly TwoOptSAParameter parameter;

    private readonly ITSPInitialSolver initialSolver;

    public TwoOptSA(TSPSolverContext context, TwoOptSAParameter parameter, ITSPInitialSolver initialSolver)
    {
        this.context = context;
        this.parameter = parameter;
        this.initialSolver = initialSolver;
    }

    public TSPAnswer Solve()
    {
        var answer = initialSolver.Solve();
        Route[] routes = answer.Routes.ToArray();
        Route[] swapTable = new Route[routes.Length];

        var random = context.Random;
        double st = parameter.StartTemp;
        double et = parameter.EndTemp;
        // const double temp_factor = 0.999;
        Span<int> buf = stackalloc int[2];

        var maxIteration = parameter.MaxIteration;
        for (int i = 0; i < maxIteration; i++)
        {
            var rand = TSPUtil.CreateRandoms(buf, routes.Length, random);
            var (oldCost, newCost) = TSPUtil.CalcSwap(context, buf[0], buf[1], routes, swapTable);

            double temp = st + (et - st) * i / maxIteration; // Math.Pow(temp_factor, i);
            double prod = Math.Exp((oldCost - newCost).TotalSeconds / temp);
            var r = random.NextDouble();
            if (prod > r)
            {
                TSPUtil.CommitSwap(buf[0], buf[1], routes, swapTable);
            }
        }

        return context.SubmitAnswer(routes);
    }
}

public record class TwoOptSAParameter(int MaxIteration, int StartTemp, int EndTemp);