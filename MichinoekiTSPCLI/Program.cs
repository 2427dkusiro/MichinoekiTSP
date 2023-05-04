
using MichinoekiTSP.Data;

namespace MichinoekiTSP.CLI;

internal class Program
{
    private static async Task Main()
    {
        var manager = MichinoekiResourceManager.CreateInstance();
        // await manager.FetchNotExistRoutes(Console.WriteLine, () => Console.ReadLine()!);
        TSPSolver solver = new(manager, manager.Michinoekis.First(x => x.Name == "三笠"));
        solver.TwoOptILS(maxIteration: 10);
    }
}