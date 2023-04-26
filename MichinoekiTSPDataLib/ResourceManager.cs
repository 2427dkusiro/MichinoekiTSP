using System.Reflection;

namespace MichinoekiTSP.Data;

public class MichinoekiResourceManager
{
    protected MichinoekiResourceManager() { }

    private GeometryPoint[] michinoekis;
    public IReadOnlyList<GeometryPoint> Michinoekis { get => michinoekis; }

    private Route[] routes;

    public IReadOnlyList<Route> Routes { get => routes; }

    protected void Initialize()
    {
        var execDir = Assembly.GetExecutingAssembly().Location;
        execDir = execDir[0..execDir.IndexOf("MichinoekiTSP")];
        string rawSaveDir = $@"{execDir}MichinoekiTSP\MichinoekiTSPDataLib\Routes\Default_Raw\";

        michinoekis = MichinoekiJsonReader.Read().ToArray();
        routes = LoadRawRoutes(rawSaveDir, michinoekis).ToArray();
    }

    public static MichinoekiResourceManager CreateInstance()
    {
        var instance = new MichinoekiResourceManager();
        instance.Initialize();
        return instance;
    }

    private static IEnumerable<Route> LoadRawRoutes(string saveDir, GeometryPoint[] michinoekiGeometries)
    {
        var files = Directory.GetFiles(saveDir, "*.json");
        foreach (var file in files)
        {
            using StreamReader reader = new(file);
            var str = reader.ReadToEnd();
            var obj = Route.FromJson(str, michinoekiGeometries);
            yield return obj;
        }
    }

    private static async Task FetchRoute(string saveDir, GeometryPoint from, GeometryPoint to)
    {
        var client = new GoogleRouteAPIClient();
        var route = await client.GetRoute(from, to);

        var path = Path.Combine(saveDir, $"'{route.From.Name}'から'{route.To.Name}'まで.json");
        using StreamWriter stream = new(path);
        var json = route.ToJson();
        stream.Write(json);
        stream.Flush();
    }
}
