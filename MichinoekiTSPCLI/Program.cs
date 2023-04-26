namespace MichinoekiTSP.CLI;

using MichinoekiTSP.Data;

class Program
{
    static async Task Main()
    {
        MichinoekiResourceManager manager = MichinoekiResourceManager.CreateInstance();
        await manager.FetchNotExistRoutes(msg => Console.WriteLine(msg), () => Console.ReadLine()!);
    }
}