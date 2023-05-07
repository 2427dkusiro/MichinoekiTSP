namespace MichinoekiTSP.Data.Solvers;

public sealed class TSPAnswer
{
    private readonly Route[] routes;

    public TSPAnswer(Route[] routes)
    {
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