namespace MichinoekiTSP.Data.Solvers;

public sealed class RepeatExecuter : ITSPExecuter
{
    private readonly TSPSolverContext context;

    private readonly ITSPExecuter executer;

    private readonly RepeatExecuterParameter parameter;

    public RepeatExecuter(TSPSolverContext context, ITSPExecuter executer, RepeatExecuterParameter parameter)
    {
        this.context = context;
        this.executer = executer;
        this.parameter = parameter;
    }

    public TSPAnswer Solve()
    {
        TSPAnswer? ans = null;
        for (int i = 0; i < parameter.Count; i++)
        {
            var nAns = executer.Solve();
            if (ans is null || ans > nAns)
            {
                ans = nAns;
            }
        }
        return ans!;
    }
}

public record class RepeatExecuterParameter(int Count);