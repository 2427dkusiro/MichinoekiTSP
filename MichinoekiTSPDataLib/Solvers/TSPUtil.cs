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

    public static bool TrySwap(TSPSolverContext context, int i, int j, Route[] routes, Route[] swapTable)
    {
        var (oldCost, newCost) = CalcSwap(context, i, j, routes, swapTable);

        if (oldCost > newCost)
        {
            CommitSwap(i, j, routes, swapTable);
            return true;
        }
        return false;
    }

    public static (TimeSpan oldCost, TimeSpan newCost) CalcSwap(TSPSolverContext context, int i, int j, Route[] routes, Route[] swapTable)
    {
        Debug.Assert(i != j);
        Debug.Assert(routes.Length == swapTable.Length);
        if (i > j)
        {
            return CalcSwap(context, j, i, routes, swapTable);
        }
        Debug.Assert(j < routes.Length);

        var a = routes[i];
        var b = routes[j];

        swapTable[i] = context.FindRoute(a.From, b.From);
        var oldCost = routes[i].Duration;
        var newCost = swapTable[i].Duration;
        for (int k = i + 1; k < j; k++)
        {
            swapTable[k] = context.FindRoute(routes[j - k + i].To, routes[j - k + i].From);
            oldCost += routes[k].Duration;
            newCost += swapTable[k].Duration;
        }
        swapTable[j] = context.FindRoute(a.To, b.To);
        oldCost += routes[j].Duration;
        newCost += swapTable[j].Duration;

        return (oldCost, newCost);
    }

    public static void CommitSwap(int i, int j, Route[] routes, Route[] swapTable)
    {
        if (i > j)
        {
            CommitSwap(j, i, routes, swapTable);
            return;
        }
        Array.Copy(swapTable, i, routes, i, j - i + 1);
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
