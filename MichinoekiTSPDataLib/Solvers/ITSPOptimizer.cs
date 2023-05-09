namespace MichinoekiTSP.Data.Solvers;

public interface ITSPOptimizer
{
    public TSPAnswer Optimize(TSPAnswer answer);
}

public sealed class TSPNullOptimizer : ITSPOptimizer
{
    public TSPAnswer Optimize(TSPAnswer answer)
    {
        return answer;
    }
}