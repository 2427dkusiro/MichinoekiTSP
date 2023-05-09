namespace MichinoekiTSP.Data.Solvers;

public sealed class SimpleLoopExecuter : ITSPExecuter
{
    private readonly TSPSolverContext context;

    private readonly SimpleLoopParameter parameter;

    private readonly ITSPInitialSolver initialSolver;

    private readonly ITSPOptimizer optimizer;

    public SimpleLoopExecuter(TSPSolverContext context, SimpleLoopParameter parameter, ITSPInitialSolver initialSolver, ITSPOptimizer optimizer)
    {
        this.context = context;
        this.parameter = parameter;
        this.initialSolver = initialSolver;
        this.optimizer = optimizer;
    }

    public TSPAnswer Solve()
    {
        var answer = initialSolver.Solve();
        var maxIteration = parameter.MaxIteration;
        for (int i = 0; i < maxIteration; i++)
        {
            answer = optimizer.Optimize(answer);
        }
        return answer;
    }
}

public record class SimpleLoopParameter(int MaxIteration);
