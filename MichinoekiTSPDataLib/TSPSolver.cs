using MichinoekiTSP.Data;

using System.Diagnostics;

namespace MichinoekiTSP.Data;

public class TSPSolver
{
    private readonly MichinoekiResourceManager manager;

    private readonly GeometryPoint start;

    private readonly Dictionary<GeometryPoint, Route[]> nextRouteDictionary;

    private readonly Dictionary<GeometryPoint, Dictionary<GeometryPoint, Route>> routeDictionary;

    public TSPSolver(MichinoekiResourceManager manager, GeometryPoint start)
    {
        this.manager = manager;
        this.start = start;
        nextRouteDictionary = manager.Routes.GroupBy(x => x.From).ToDictionary(x => x.Key, x => x.ToArray());
        routeDictionary = manager.Routes.GroupBy(x => x.From).ToDictionary(x => x.Key, x => x.ToDictionary(y => y.To, y => y));
    }

    public TSPAnswer NearestNeighbor()
    {
        Dictionary<GeometryPoint, bool> stationDict = manager.Michinoekis.ToDictionary(x => x, _ => false);
        int count = stationDict.Count;
        if (stationDict.ContainsKey(start))
        {
            stationDict[start] = true;
            count--;
        }
        var ans = new Route[count];

        Route NonVisitedMin(Route[] routes)
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

        var current = start;
        for (int i = 0; i < ans.Length; i++)
        {
            var selected = NonVisitedMin(nextRouteDictionary[current]);
            current = selected!.To;
            stationDict[current] = true;
            ans[i] = selected;
        }

        return new TSPAnswer(ans);
    }

    public TSPAnswer TwoOptLS(TSPAnswer? answer = null, int stepLimit = 100)
    {
        answer ??= NearestNeighbor();
        var routes = answer.Routes.ToArray();
        var swapTable = new Route[routes.Length];

        bool TrySwap(int i, int j)
        {
            Debug.Assert(i != j);

            var a = routes[i];
            var b = routes[j];

            var diff = j - i;
            swapTable[0] = routeDictionary[a.From][b.From];
            for (int k = 1; k < diff; k++)
            {
                swapTable[k] = routeDictionary[routes[j - k].To][routes[j - k].From];
            }
            swapTable[diff] = routeDictionary[a.To][b.To];

            var oldCost = TimeSpan.Zero;
            var newCost = TimeSpan.Zero;
            for (int k = 0; k <= diff; k++)
            {
                oldCost += routes[i + k].Duration;
                newCost += swapTable[k].Duration;
            }

            if (oldCost <= newCost)
            {
                return false;
            }

            // commit
            for (int k = 0; k <= diff; k++)
            {
                routes[i + k] = swapTable[k];
            }
            return true;
        }

        int step = 0;
        void Log()
        {
            Debug.WriteLine($"===step {step}===");
            Debug.WriteLine($"時間:{answer.TotalTime}");
            Debug.WriteLine($"距離:{answer.TotalDistance / 1000m}km");
        }
#if DEBUG
        answer = new TSPAnswer(routes);
#endif

        for (step = 1; step <= stepLimit; step++)
        {
            bool hasSwap = false;
            for (int i = 0; i < routes.Length - 1; i++)
            {
                for (int j = i + 1; j < routes.Length - 1; j++)
                {
                    hasSwap |= TrySwap(i, j);
                }
            }
            if (!hasSwap)
            {
                Debug.WriteLine($"2-optは{step - 1}stepsで極大値に達しました");
                break;
            }

#if DEBUG
            answer = new TSPAnswer(routes);
            Log();
#endif
        }
        return new TSPAnswer(routes);
    }

    public TSPAnswer TwoOptILS(TSPAnswer? answer = null, int maxIteration = 100)
    {
        answer ??= TwoOptLS();
        Route[] routes = answer.Routes.ToArray();
        Random random = new();

        void Kick()
        {
            var offset = new int[routes.Length];
            Span<int> target = stackalloc int[4];
            for (int i = 0; i < 4; i++)
            {
                bool used = false;
                var rand = random.Next(routes.Length - i);
                for (int j = 0; j < i; j++)
                {
                    if (target[j] == rand)
                    {
                        used = true;
                    }
                }
                if (used)
                {
                    i--;
                    continue;
                }
                target[i] = rand;
            }
            Debug.Assert(target.ToArray().Distinct().Count() == target.Length);
            target.Sort();

            var arr = new Route[4];
            arr[0] = routeDictionary[routes[target[0]].From][routes[target[2]].To];
            arr[1] = routeDictionary[routes[target[1]].From][routes[target[3]].To];
            arr[2] = routeDictionary[routes[target[2]].From][routes[target[0]].To];
            arr[3] = routeDictionary[routes[target[3]].From][routes[target[1]].To];

            int current = 0;
            var newArray = new Route[routes.Length];
            for (int i = 0; i < newArray.Length; i++)
            {
                if (target.Contains(current))
                {
                    var obj = arr[target.IndexOf(current)];
                    newArray[i] = obj;
                    current = routes.Select((x, i) => (x, i)).First(x => x.x.To == obj.To).i + 1;
                }
                else
                {
                    if ((uint)current >= routes.Length)
                    {

                    }
                    newArray[i] = routes[current++];
                }
            }
            routes = newArray;
            Debug.Assert(routes.Select(x => x.From).Distinct().Count() == routes.Count());
        }

        for (int i = 0; i < maxIteration; i++)
        {
            Kick();
            var nextAns = TwoOptLS(new TSPAnswer(routes));
            if (answer.TotalTime > nextAns.TotalTime)
            {
                answer = nextAns;
            }
        }
        return answer;
    }
}

public class TSPAnswer
{
    private readonly Route[] routes;

    public TSPAnswer(Route[] routes)
    {
        Debug.Assert(routes.Select(x => x.From).Distinct().Count() == routes.Count());
        Debug.Assert(routes.Take(routes.Count() - 1).Select((x, i) => (x, i)).All(o => o.x.To == routes[o.i + 1].From));

        this.routes = routes;
    }

    public IReadOnlyList<Route> Routes { get => routes; }

    public long TotalDistance
    {
        get
        {
            var total = 0l;
            foreach (var r in routes)
            {
                total += r.DistanceMeters;
            }
            return total;
        }
    }

    public TimeSpan TotalTime
    {
        get
        {
            var total = TimeSpan.Zero;
            foreach (var r in routes)
            {
                total += r.Duration;
            }
            return total;
        }
    }
}
