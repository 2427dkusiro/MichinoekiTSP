using MichinoekiTSP.Data.Solvers;

namespace MichinoekiTSP.Data.Solvers;

public class TSPSolverBuilder
{
    public TSPSolverBuilder() { }

    public ITSPInitialSolver? InitialSolver { get; private set; }

    public ITSPOptimizer? Optimizer { get; private set; }

    public ITSPExecuter? Executer { get; private set; }

    public List<object> Dependencies { get; } = new();

    public TSPSolverBuilder AddParameter(object obj)
    {
        Dependencies.Add(obj);
        return this;
    }

    private static T BuildInstance<T>(IEnumerable<object> list)
    {
        var ctors = typeof(T).GetConstructors();
        if (ctors.Length != 1)
        {
            throw new NotSupportedException();
        }
        var args = ctors[0].GetParameters().Select(x =>
        {
            var type = x.ParameterType;
            return list.FirstOrDefault(x => x.GetType().IsAssignableTo(type))
                ?? throw new InvalidOperationException($"cannot construct type {typeof(T)}. param {type} does not registered.");
        });
        return (T)ctors[0].Invoke(args.ToArray());
    }

    private IEnumerable<object> BuildDeps(IEnumerable<object> param)
    {
        IEnumerable<object> exists = new object?[] { InitialSolver, Optimizer, Executer }.Where(x => x is not null)!;
        var deps = Dependencies.Concat(param).Concat(exists);
        return deps;
    }

    public TSPSolverBuilder UseInitialSolver<T>(params object[] objects) where T : ITSPInitialSolver
    {
        var deps = BuildDeps(objects);
        InitialSolver = BuildInstance<T>(deps);
        return this;
    }

    public TSPSolverBuilder UseOptimizer<T>(params object[] objects) where T : ITSPOptimizer
    {
        var deps = BuildDeps(objects);
        Optimizer = BuildInstance<T>(deps);
        return this;
    }

    public TSPSolverBuilder UseExecuter<T>(params object[] objects) where T : ITSPExecuter
    {
        var deps = BuildDeps(objects);
        Executer = BuildInstance<T>(deps);
        return this;
    }

    public Func<TSPAnswer> Build()
    {
        if (Executer is not null)
        {
            return Executer.Solve;
        }
        if (InitialSolver is null)
        {
            throw new InvalidOperationException();
        }
        Optimizer ??= new TSPNullOptimizer();
        Executer = new SingleExecuter(InitialSolver, Optimizer);
        return Executer.Solve;
    }
}
