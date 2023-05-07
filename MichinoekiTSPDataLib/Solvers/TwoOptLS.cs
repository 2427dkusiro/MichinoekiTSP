namespace MichinoekiTSP.Data.Solvers;

public class TwoOptLS : ITSPOptimizer
{
    public static Type? RequiredParameterType => typeof(TwoOptLSParameter);

    public static TSPAnswer Optimize(TSPSolverContext context, TSPAnswer answer)
    {
        var p = context.GetParameter<TwoOptLSParameter>();
        var routes = answer.Routes.ToArray();
        var swapTable = new Route[routes.Length];

        int step;
        void Log()
        {
            if (!context.DoValidateAnswer)
            {
                return;
            }
            // validate answer
            answer = new TSPAnswer(routes);
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
