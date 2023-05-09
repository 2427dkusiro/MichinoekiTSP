namespace MichinoekiTSP.Data.Solvers;

public interface ITSPExecuter
{
    public TSPAnswer Solve();
}

public sealed class SingleExecuter : ITSPExecuter
{
    private readonly ITSPInitialSolver initialSolver;

    private readonly ITSPOptimizer optimizer;

    public SingleExecuter(ITSPInitialSolver initialSolver, ITSPOptimizer optimizer)
    {
        this.initialSolver = initialSolver;
        this.optimizer = optimizer;
    }

    public TSPAnswer Solve()
    {
        var ans = initialSolver.Solve();
        return optimizer.Optimize(ans);
    }
}