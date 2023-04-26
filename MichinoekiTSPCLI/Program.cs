
using MichinoekiTSP.Data;

namespace MichinoekiTSP.CLI;

internal class Program
{
    private static async Task Main()
    {
        var manager = MichinoekiResourceManager.CreateInstance();
        // await manager.FetchNotExistRoutes(Console.WriteLine, () => Console.ReadLine()!);
    }
}