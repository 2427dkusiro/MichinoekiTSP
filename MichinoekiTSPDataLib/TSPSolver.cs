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

        int step = 0;

#if DEBUG
        void Log()
        {
            Debug.WriteLine($"===step {step}===");
            Debug.WriteLine($"時間:{answer.TotalTime}");
            Debug.WriteLine($"距離:{answer.TotalDistance / 1000m}km");
        }
        answer = new TSPAnswer(routes);
#endif

        for (step = 1; step <= stepLimit; step++)
        {
            bool hasSwap = false;
            for (int i = 0; i < routes.Length - 1; i++)
            {
                for (int j = i + 1; j < routes.Length - 1; j++)
                {
                    hasSwap |= TrySwap(i, j, routes, swapTable);
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


    public TSPAnswer TwoOptILS(TSPAnswer? answer = null, Random? random = null, int maxIteration = 100)
    {
        answer ??= TwoOptLS();
        Route[] routes = answer.Routes.ToArray();

        for (int i = 0; i < maxIteration; i++)
        {
            routes = Kick(routes, random);
            var nextAns = TwoOptLS(new TSPAnswer(routes));
            if (answer.TotalTime > nextAns.TotalTime)
            {
                answer = nextAns;
            }
        }
        return answer;
    }

    public TSPAnswer TwoOptSA(TSPAnswer? answer = null, Random? random = null, double? startTemp = null, double? endTemp = null, int maxIteration = 1_000_000)
    {
        answer ??= TwoOptLS();
        Route[] routes = answer.Routes.ToArray();
        Route[] swapTable = new Route[routes.Length];

        random ??= new();
        double st = startTemp ??= 5000;
        double et = endTemp ??= 10;
        // const double temp_factor = 0.999;
        Span<int> buf = stackalloc int[2];

        for (int i = 0; i < maxIteration; i++)
        {
            var rand = TSPUtil.Randoms(buf, routes.Length, random);
            var (oldCost, newCost) = CalcSwap(buf[0], buf[1], routes, swapTable);

            double temp = st + (et - st) * i / maxIteration; // Math.Pow(temp_factor, i);
            double prod = Math.Exp((oldCost - newCost).TotalSeconds / temp);
            var r = random.NextDouble();
            if (prod > r)
            {
                CommitSwap(buf[0], buf[1], routes, swapTable);
            }
        }

        return new TSPAnswer(routes);
    }

    // lib

    private bool TrySwap(int i, int j, Route[] routes, Route[] swapTable)
    {
        var (oldCost, newCost) = CalcSwap(i, j, routes, swapTable);

        if (oldCost > newCost)
        {
            CommitSwap(i, j, routes, swapTable);
            return true;
        }
        return false;
    }

    private (TimeSpan oldCost, TimeSpan newCost) CalcSwap(int i, int j, Route[] routes, Route[] swapTable)
    {
        Debug.Assert(i != j);
        Debug.Assert(routes.Length == swapTable.Length);
        if (i > j)
        {
            return CalcSwap(j, i, routes, swapTable);
        }
        Debug.Assert(j < routes.Length);

        var a = routes[i];
        var b = routes[j];

        swapTable[i] = routeDictionary[a.From][b.From];
        var oldCost = routes[i].Duration;
        var newCost = swapTable[i].Duration;
        for (int k = i + 1; k < j; k++)
        {
            swapTable[k] = routeDictionary[routes[j - k + i].To][routes[j - k + i].From];
            oldCost += routes[k].Duration;
            newCost += swapTable[k].Duration;
        }
        swapTable[j] = routeDictionary[a.To][b.To];
        oldCost += routes[j].Duration;
        newCost += swapTable[j].Duration;

        return (oldCost, newCost);
    }

    private void CommitSwap(int i, int j, Route[] routes, Route[] swapTable)
    {
        if (i > j)
        {
            CommitSwap(j, i, routes, swapTable);
            return;
        }
        Array.Copy(swapTable, i, routes, i, j - i + 1);
    }

    private Route[] Kick(Route[] routes, Random? random)
    {
        Span<int> target = stackalloc int[4];
        TSPUtil.Randoms(target, routes.Length, random);
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
                Debug.Assert((uint)current < routes.Length);
                newArray[i] = routes[current++];
            }
        }

        Debug.Assert(newArray.Select(x => x.From).Distinct().Count() == routes.Length);
        return newArray;
    }
}

public static class TSPUtil
{
    public static Span<int> Randoms(Span<int> buffer, int maxValue, Random? random = null)
    {
        random ??= new();
        for (int i = 0; i < buffer.Length; i++)
        {
            int rand;
            do
            {
                rand = random.Next(maxValue);
            } while (buffer.Contains(rand));
            buffer[i] = rand;
        }
        return buffer;
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
