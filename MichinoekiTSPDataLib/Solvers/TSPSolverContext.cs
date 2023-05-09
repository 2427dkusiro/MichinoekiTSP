using System.Diagnostics;

namespace MichinoekiTSP.Data.Solvers;

public sealed class TSPSolverContext
{
    private readonly Dictionary<GeometryPoint, Route[]> nextRouteDictionary;

    private readonly Dictionary<GeometryPoint, Dictionary<GeometryPoint, Route>> routeDictionary;

    public TSPSolverContext(MichinoekiResourceManager manager, GeometryPoint start, Random random)
    {
        StartPoint = start;
        Michinoekis = manager.Michinoekis.ToArray();
        nextRouteDictionary = manager.Routes.GroupBy(x => x.From).ToDictionary(x => x.Key, x => x.ToArray());
        routeDictionary = manager.Routes.GroupBy(x => x.From).ToDictionary(x => x.Key, x => x.ToDictionary(y => y.To, y => y));
        Random = random;
    }

    public GeometryPoint StartPoint { get; }

    public GeometryPoint[] Michinoekis { get; }

    public Route[] FindEdgeFrom(GeometryPoint point)
    {
        return nextRouteDictionary[point];
    }

    public Route FindRoute(GeometryPoint from, GeometryPoint to)
    {
        return routeDictionary[from][to];
    }

    public Random Random { get; }

    public bool DoValidateAnswer { get; }

    public TSPAnswer SubmitAnswer(Route[] routes)
    {
        if (DoValidateAnswer)
        {
            ValidateAnswer(routes);
        }
        return new TSPAnswer(routes);
    }

    private static void ValidateAnswer(Route[] routes)
    {
        HashSet<Route> valid = new HashSet<Route>();
        GeometryPoint? prev = null;

        foreach (var route in routes)
        {
            if (valid.Contains(route))
            {
                throw new InvalidDataException($"answer contains same route: {route.From}~{route.To}");
            }
            if (prev is not null)
            {
                if (route.From != prev)
                {
                    throw new InvalidDataException($"answer is not continuous: {prev.Name}/{route.From}");
                }
            }
            prev = route.To;
        }
    }


    public void WriteMessage(string msg)
    {
        Debug.WriteLine(msg);
    }
}
