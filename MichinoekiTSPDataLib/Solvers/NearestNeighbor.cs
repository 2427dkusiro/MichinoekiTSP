namespace MichinoekiTSP.Data.Solvers;

public class NearestNeighbor : ITSPInitialSolver
{
    private readonly TSPSolverContext context;

    public NearestNeighbor(TSPSolverContext context)
    {
        this.context = context;
    }

    public TSPAnswer Solve()
    {
        Dictionary<GeometryPoint, bool> stationDict = context.Michinoekis.ToDictionary(x => x, _ => false);
        int count = stationDict.Count;
        if (stationDict.ContainsKey(context.StartPoint))
        {
            stationDict[context.StartPoint] = true;
            count--;
        }
        var ans = new Route[count];

        static Route NonVisitedMin(Route[] routes, Dictionary<GeometryPoint, bool> stationDict)
        {
            Route? min = null;
            foreach (var r in routes)
            {
                if (stationDict[r.To])
                {
                    continue;
                }
                if (min is null)
                {
                    min = r;
                    continue;
                }
                if (min.Duration > r.Duration)
                {
                    min = r;
                    continue;
                }
            }
            return min!;
        }

        var current = context.StartPoint;
        for (int i = 0; i < ans.Length; i++)
        {
            var selected = NonVisitedMin(context.FindEdgeFrom(current), stationDict);
            current = selected!.To;
            stationDict[current] = true;
            ans[i] = selected;
        }

        return context.SubmitAnswer(ans);
    }
}
