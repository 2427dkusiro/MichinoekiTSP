namespace MichinoekiTSP.Data.Solvers;

public sealed class TSPAnswer : IComparable<TSPAnswer>
{
    private readonly Route[] routes;

    public TSPAnswer(Route[] routes)
    {
        this.routes = routes;
    }

    public ReadOnlySpan<Route> Routes { get => routes; }

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

    public int CompareTo(TSPAnswer? other)
    {
        return TotalTime.CompareTo(other?.TotalTime);
    }

    public static bool operator <(TSPAnswer left, TSPAnswer right)
    {
        return left.CompareTo(right) < 0;
    }

    public static bool operator >(TSPAnswer left, TSPAnswer right)
    {
        return left.CompareTo(right) > 0;
    }

    public static bool operator <=(TSPAnswer left, TSPAnswer right)
    {
        return left.CompareTo(right) <= 0;
    }

    public static bool operator >=(TSPAnswer left, TSPAnswer right)
    {
        return left.CompareTo(right) >= 0;
    }
}