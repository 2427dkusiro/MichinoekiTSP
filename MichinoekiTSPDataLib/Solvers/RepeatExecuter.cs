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
        if (parameter.ParallelCount == 1)
        {
            for (int i = 0; i < parameter.RepeatCount; i++)
            {
                var nAns = executer.Solve();
                if (ans is null || ans > nAns)
                {
                    ans = nAns;
                }
            }
        }
        else
        {
            var option = new ParallelOptions();
            object lockObj = new();
            if (parameter.ParallelCount > 1)
            {
                option.MaxDegreeOfParallelism = parameter.ParallelCount;
            }
            Parallel.For(0, parameter.RepeatCount, option, arg =>
            {
                var nAns = executer.Solve();
                lock (lockObj)
                {
                    if (ans is null || ans > nAns)
                    {
                        ans = nAns;
                    }
                }
            });
        }
        return ans!;
    }
}

public record class RepeatExecuterParameter(int RepeatCount, int ParallelCount = 0);