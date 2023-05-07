namespace MichinoekiTSP.Data.Solvers;


public class DoubleBridge<T> : ITSPOptimizer where T : ITSPOptimizer
{
    public static Type? RequiredParameterType => null;

    public static TSPAnswer Optimize(TSPSolverContext context, TSPAnswer answer)
    {
        var routes = TSPUtil.Kick(context, answer.Routes.ToArray(), context.Random);
        var next = T.Optimize(context, context.SubmitAnswer(routes));
        return answer.TotalTime > next.TotalTime ? next : answer;
    }
}


public class SimpleLoopExecuter<TInit, TOpt> : ITSPExecuter where TInit : ITSPInitialSolver where TOpt : ITSPOptimizer
{
    public static Type? RequiredParameterType => typeof(SimpleLoopParameter);

    public static TSPAnswer Solve(TSPSolverContext context)
    {
        var answer = TInit.Solve(context);
        var maxIteration = context.GetParameter<SimpleLoopParameter>().MaxIteration;
        for (int i = 0; i < maxIteration; i++)
        {
            answer = TOpt.Optimize(context, answer);
        }
        return answer;
    }
}


public record class SimpleLoopParameter(int MaxIteration);
