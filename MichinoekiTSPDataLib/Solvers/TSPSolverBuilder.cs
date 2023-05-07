using MichinoekiTSP.Data.Solvers;

namespace MichinoekiTSPDataLib.Solvers;

public class TSPSolverBuilder
{
    public TSPSolverBuilder() { }

    public Type? InitialSolver { get; set; }

    public object? InitialSolverParam { get; set; }

    public Type? Optimizer { get; set; }

    public object? OptimizerParam { get; set; }

    public Type? Executer { get; set; }

    public object? ExecuterParam { get; set; }

    public TSPSolverBuilder UseInitialSolver<T>(object? parameter = null) where T : ITSPInitialSolver
    {
        if (InitialSolver is not null)
        {
            throw new InvalidOperationException();
        }
        InitialSolver = typeof(T);
        if (T.RequiredParameterType != parameter?.GetType())
        {
            throw new ArgumentException("parameter required", nameof(parameter));
        }
        InitialSolverParam = parameter;
        return this;
    }

    public TSPSolverBuilder UseOptimizer<T>(object? parameter = null) where T : ITSPOptimizer
    {
        if (Optimizer is not null)
        {
            throw new InvalidOperationException();
        }
        Optimizer = typeof(T);
        if (T.RequiredParameterType != parameter?.GetType())
        {
            throw new ArgumentException("parameter required", nameof(parameter));
        }
        OptimizerParam = parameter;
        return this;
    }

    public TSPSolverBuilder UseExecuter<T>(object? parameter = null) where T : ITSPExecuter
    {
        if (Executer is not null)
        {
            throw new InvalidOperationException();
        }
        Executer = typeof(T);
        if (T.RequiredParameterType != parameter?.GetType())
        {
            throw new ArgumentException("parameter required", nameof(parameter));
        }
        ExecuterParam = parameter;
        return this;
    }

    public void Build()
    {
        if (InitialSolver is null)
        {
            throw new InvalidOperationException("");
        }
        if (Optimizer is null)
        {
            if()
        }
    }
}
