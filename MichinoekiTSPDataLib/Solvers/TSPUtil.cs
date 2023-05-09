using System.Diagnostics;

namespace MichinoekiTSP.Data.Solvers;

public static class TSPUtil
{
    public static Span<int> CreateRandoms(Span<int> buffer, int maxValue, Random? random = null)
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

    public static bool TrySwap(TSPSolverContext context, int i, int j, Span<Route> routes, Route[] swapTable)
    {
        var (oldCost, newCost) = CalcSwap(context, i, j, routes, swapTable);

        if (oldCost > newCost)
        {
            CommitSwap(i, j, routes, swapTable);
            return true;
        }
        return false;
    }

    public static (TimeSpan oldCost, TimeSpan newCost) CalcSwap(TSPSolverContext context, int i, int j, ReadOnlySpan<Route> routes, Route[] swapTable)
    {
        Debug.Assert(i != j);
        Debug.Assert(routes.Length == swapTable.Length);
        if (i > j)
        {
            (i, j) = (j, i);
        }
        Debug.Assert(j < routes.Length);

        var a = routes[i];
        var b = routes[j];
        var newRouteA = context.FindRoute(a.From, b.From);
        var newRouteB = context.FindRoute(a.To, b.To);

        swapTable[i] = newRouteA;
        var oldCost = a.Duration;
        var newCost = newRouteA.Duration;

        var routeSlice = routes[(i + 1)..j];
        var swapTableSlice = swapTable.AsSpan()[(i + 1)..j];
        for (int k = 0; k < routeSlice.Length; k++)
        {
            var target = routeSlice[^(k + 1)];
            swapTableSlice[k] = context.FindRoute(target.To, target.From);
            oldCost += routeSlice[k].Duration;
            newCost += swapTableSlice[k].Duration;
        }
        swapTable[j] = newRouteB;
        oldCost += b.Duration;
        newCost += newRouteB.Duration;

        return (oldCost, newCost);
    }

    public static void CommitSwap(int i, int j, Span<Route> routes, Route[] swapTable)
    {
        if (i > j)
        {
            CommitSwap(j, i, routes, swapTable);
            return;
        }

        swapTable.AsSpan()[i..(j + 1)].CopyTo(routes[i..]);
    }

    public static Route[] Kick(TSPSolverContext context, Route[] routes, Random? random)
    {
        Span<int> target = stackalloc int[4];
        CreateRandoms(target, routes.Length, random);
        target.Sort();

        var arr = new Route[4];
        arr[0] = context.FindRoute(routes[target[0]].From, routes[target[2]].To);
        arr[1] = context.FindRoute(routes[target[1]].From, routes[target[3]].To);
        arr[2] = context.FindRoute(routes[target[2]].From, routes[target[0]].To);
        arr[3] = context.FindRoute(routes[target[3]].From, routes[target[1]].To);

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
