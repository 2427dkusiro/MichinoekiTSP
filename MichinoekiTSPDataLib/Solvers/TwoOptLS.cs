namespace MichinoekiTSP.Data.Solvers;

public sealed class TwoOptLS : ITSPOptimizer
{
    private readonly TSPSolverContext context;

    private readonly TwoOptLSParameter parameter;

    public TwoOptLS(TSPSolverContext context, TwoOptLSParameter parameter)
    {
        this.context = context;
        this.parameter = parameter;
    }

    public TSPAnswer Optimize(TSPAnswer answer)
    {
        var p = parameter;
        Route[] routes = answer.Routes.ToArray();
        var swapTable = new Route[routes.Length];

        int step;
        void Log()
        {
            if (!context.DoValidateAnswer)
            {
                return;
            }
            // validate answer
            answer = new TSPAnswer(routes.ToArray());
            context.WriteMessage($"[LS]===step {step}===");
            context.WriteMessage($"[LS]時間:{answer.TotalTime}");
            context.WriteMessage($"[LS]距離:{answer.TotalDistance / 1000m}km");
        }

        for (step = 1; step <= p.MaxSteps; step++)
        {
            bool hasSwap = false;
            for (int i = 0; i < routes.Length - 1; i++)
            {
                for (int j = i + 1; j < routes.Length - 1; j++)
                {
                    hasSwap |= TSPUtil.TrySwap(context, i, j, routes, swapTable);
                }
            }
            if (!hasSwap)
            {
                context.WriteMessage($"[LS]2-optは{step - 1}stepsで極大値に達しました");
                break;
            }

            // Log();
        }
        return context.SubmitAnswer(routes);
    }
}

public record class TwoOptLSParameter(int MaxSteps);
