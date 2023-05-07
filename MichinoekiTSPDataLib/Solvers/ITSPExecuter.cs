namespace MichinoekiTSP.Data.Solvers;

public interface ITSPExecuter
{
    public static abstract TSPAnswer Solve(TSPSolverContext context);

    public static abstract Type? RequiredParameterType { get; }
}