using System.Reflection;
using System.Text.Json;

namespace MichinoekiTSP.Data;

public static class MichinoekiJsonReader
{
    public static IEnumerable<MichinoekiGeometry> Read()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(path))
        {
            throw new NotSupportedException();
        }
        using var streamMain = File.OpenRead(Path.Combine(path, "./Resources/p35_18_01.geojson"));
        using var streamDiff = File.OpenRead(Path.Combine(path, "./Resources/Michinoeki_diff.json"));
        using var streamAbolish = File.OpenRead(Path.Combine(path, "./Resources/Michinoeki_Abolish_List.txt"));
        var dataMain = ReadFile(streamMain);
        var dataDiff = ReadFile(streamDiff);
        var listAbolish = ReadList(streamAbolish).ToList();

        return dataMain.Concat(dataDiff).Where(station => !listAbolish.Contains(station.Name));
    }

    public static IEnumerable<MichinoekiGeometry> ReadFile(Stream stream)
    {
        var json = JsonDocument.Parse(stream);
        var data = json.RootElement.GetProperty("features").EnumerateArray().Select(obj =>
        {
            var prop = obj.GetProperty("properties");
            var geo = obj.GetProperty("geometry").GetProperty("coordinates").EnumerateArray().ToArray();
            if (geo.Length != 2)
            {
                throw new NotSupportedException("geometry 'point' must has two element.");
            }
            var lat = geo[1].GetDouble();
            var lng = geo[0].GetDouble();
            var name = prop.GetProperty("道の駅名").GetString() ?? throw new FormatException("property '道の駅名' must be not null;");

            return new MichinoekiGeometry(name, lat, lng);
        });

        return data;
    }

    public static IEnumerable<string> ReadList(Stream stream)
    {
        StreamReader reader = new StreamReader(stream);
        while (true)
        {
            var line = reader.ReadLine()?.Trim();
            if (line is null)
            {
                break;
            }
            if (line.StartsWith("//") || string.IsNullOrWhiteSpace(line))
            {
                continue;
            }
            yield return line;
        }
    }
}
