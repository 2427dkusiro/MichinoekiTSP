using System.Reflection;

namespace MichinoekiTSP.Data;


public class MichinoekiResourceManager
{
    protected MichinoekiResourceManager() { }

    private GeometryPoint[] michinoekis;
    public IReadOnlyList<GeometryPoint> Michinoekis => michinoekis;

    private Route[] routes;

    public IReadOnlyList<Route> Routes => routes;

    private string rawSaveDir;

    protected void Initialize()
    {
        var execDir = Assembly.GetExecutingAssembly().Location;
        execDir = execDir[0..execDir.IndexOf("MichinoekiTSP")];
        rawSaveDir = $@"{execDir}MichinoekiTSP\MichinoekiTSPDataLib\Routes\Default_Raw\";

        michinoekis = MichinoekiJsonReader.Read().ToArray();
        routes = LoadRawRouteArray(rawSaveDir, michinoekis);
    }

    private static MichinoekiResourceManager _instance;

    public static MichinoekiResourceManager CreateInstance()
    {
        if (_instance is null)
        {
            _instance = new MichinoekiResourceManager();
            _instance.Initialize();
        }
        return _instance;
    }

    private static IEnumerable<Route> LoadRawRoutes(string saveDir, GeometryPoint[] michinoekiGeometries)
    {
        var files = Directory.GetFiles(saveDir, "*.json");
        foreach (var file in files)
        {
            using FileStream stream = new(file, FileMode.Open);
            var obj = Route.FromJson(stream, michinoekiGeometries);
            yield return obj;
        }
    }

    private static Route[] LoadRawRouteArray(string saveDir, GeometryPoint[] michinoekiGeometries)
    {
        var files = Directory.GetFiles(saveDir, "*.json");
        var result = new Route[files.Length];
        Parallel.For(0, files.Length, i =>
        {
            using FileStream stream = new(files[i], FileMode.Open);
            var obj = Route.FromJson(stream, michinoekiGeometries);
            result[i] = obj;
        });
        return result;
    }

    public async Task FetchNotExistRoutes(Action<string> writeMsg, Func<string> getInput)
    {
        GeometryPoint[] michinoekiGeometries = michinoekis;
        var saveDir = rawSaveDir;

        List<(GeometryPoint, GeometryPoint)> list = new();
        var existHitCount = 0;
        for (var i = 0; i < michinoekiGeometries.Length; i++)
        {
            for (var j = 0; j < michinoekiGeometries.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }
                GeometryPoint from = michinoekiGeometries[i];
                GeometryPoint to = michinoekiGeometries[j];
                if (routes.Any(r => r.From == from && r.To == to))
                {
                    existHitCount++;
                    continue;
                }

                list.Add((from, to));
            }
        }

        var updateCount = list.Count;
        const decimal requestFee = 0.67m;
        var estimateCost = requestFee * updateCount;
        var msg = $"""
            不足している経路情報を取得します。
            {existHitCount}個の経路が既にキャッシュされています。
            {updateCount}個の経路を新しく取得します。
            {estimateCost}円のAPI利用料金が概算で請求されます。
            続行しますか(y/n)？
            """;
        writeMsg(msg);
        var resp = getInput();
        if (resp != "y")
        {
            return;
        }

        var client = new GoogleRouteAPIClient();
        for (var i = 0; i < list.Count; i++)
        {
            (GeometryPoint from, GeometryPoint to) = list[i];
            writeMsg($"[{i + 1}/{list.Count}] begin fetch route '{from.Name}' to '{to.Name}' ...");
            await FetchRoute(client, saveDir, from, to, writeMsg);
            writeMsg($"[{i + 1}/{list.Count}] fetch completed!");
        }
    }

    private static async Task FetchRoute(GoogleRouteAPIClient client, string saveDir, GeometryPoint from, GeometryPoint to, Action<string>? writeLog = null)
    {
        Route route = await client.GetRoute(from, to);

        var path = Path.Combine(saveDir, $"'{Escape(route.From.Name)}'から'{Escape(route.To.Name)}'まで.json");
        using StreamWriter stream = new(path);
        var json = route.ToJson();
        stream.Write(json);
        stream.Flush();
    }

    private static string Escape(string str)
    {
        IList<char> prohibited = new char[] { '"', '<', '>', '｜', ':', '*', '?', '¥', '/' };
        return string.Concat(str.Select(c => prohibited.Contains(c) ? $"u+{(int)c:x}" : c.ToString()));
    }
}
