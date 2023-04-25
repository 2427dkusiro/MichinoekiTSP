namespace MichinoekiTSP.CLI;

using MichinoekiTSP.Data;

using System.Reflection;

class Program
{
    static void Main()
    {
        var execPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        const string rawSaveDir = @"C:\Users\Kota\Source\Repos\2427dkusiro\MichinoekiTSP\MichinoekiTSPDataLib\Routes\Default_Raw\";
        var credential = Path.Combine(execPath!, "google_credential.json");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential);

        var data = MichinoekiJsonReader.Read().ToArray();

        var sta1 = data.First(x => x.Name == "三笠");
        var sta2 = data.First(x => x.Name == "スタープラザ 芦別");

        // FetchRoute(rawSaveDir, sta1, sta2);
        var routes = Load(rawSaveDir, data).ToArray();
        var str = $"""[{string.Join(',', routes.First().PolylineDecoded.Select(x => $"[{x.Latitude},{x.Longitude}]"))}]""";
    }

    private static IEnumerable<Route> Load(string saveDir, MichinoekiGeometry[] michinoekiGeometries)
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

    private static void FetchRoute(string saveDir, MichinoekiGeometry from, MichinoekiGeometry to)
    {
        var route = GoogleRouteAPI.GetRoute(from, to);

        var path = Path.Combine(saveDir, $"'{route.From.Name}'から'{route.To.Name}'まで.json");
        using StreamWriter stream = new(path);
        var json = route.ToJson();
        stream.Write(json);
        stream.Flush();
    }
}