namespace MichinoekiTSP.CLI;

using MichinoekiTSP.Data;

class Program
{
    static void Main()
    {
        MichinoekiResourceManager manager = MichinoekiResourceManager.CreateInstance();

        var sta1 = manager.Michinoekis.First(x => x.Name == "三笠");
        var sta2 = manager.Michinoekis.First(x => x.Name == "スタープラザ 芦別");

        // FetchRoute(rawSaveDir, sta1, sta2);
        var routes = manager.Routes;
        var str = $"""[{string.Join(',', routes[0].PolylineDecoded.Select(x => $"[{x.Latitude},{x.Longitude}]"))}]""";
    }
}