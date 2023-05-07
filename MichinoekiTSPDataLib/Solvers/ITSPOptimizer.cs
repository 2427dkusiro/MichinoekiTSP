namespace MichinoekiTSP.Data.Solvers;

public interface ITSPOptimizer
{
    public static abstract TSPAnswer Optimize(TSPSolverContext context, TSPAnswer answer);

    public static abstract Type? RequiredParameterType { get; }
}

public class TSPNullOptimizer : ITSPOptimizer
{
    public static Type? RequiredParameterType => null;

    public static TSPAnswer Optimize(TSPSolverContext context, TSPAnswer answer)
    {
        return answer;
    }
}