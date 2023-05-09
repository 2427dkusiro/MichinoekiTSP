namespace MichinoekiTSP.Data.Solvers;

public sealed class DoubleBridgeOptimizer : ITSPOptimizer
{
    private readonly TSPSolverContext context;

    private readonly ITSPOptimizer optimizer;

    public DoubleBridgeOptimizer(TSPSolverContext context, ITSPOptimizer optimizer)
    {
        this.context = context;
        this.optimizer = optimizer;
    }

    public TSPAnswer Optimize(TSPAnswer answer)
    {
        var routes = TSPUtil.Kick(context, answer.Routes.ToArray(), context.Random);
        var next = optimizer.Optimize(context.SubmitAnswer(routes));
        return answer > next ? next : answer;
    }
}
