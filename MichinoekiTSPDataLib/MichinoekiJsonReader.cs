using System.Reflection;
using System.Text.Json;

namespace MichinoekiTSP.Data;

/// <summary>
/// オープンデータの道の駅情報を読み取る機能を提供します。
/// </summary>
public static class MichinoekiJsonReader
{
    /// <summary>
    /// 差分データを含む、道の駅情報を読み取ります。
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    public static IEnumerable<GeometryPoint> Read()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (string.IsNullOrEmpty(path))
        {
            throw new NotSupportedException();
        }
        using FileStream streamMain = File.OpenRead(Path.Combine(path, "./Resources/p35_18_01.geojson"));
        using FileStream streamDiff = File.OpenRead(Path.Combine(path, "./Resources/Michinoeki_diff.json"));
        using FileStream streamAbolish = File.OpenRead(Path.Combine(path, "./Resources/Michinoeki_Abolish_List.txt"));
        IEnumerable<GeometryPoint> dataMain = ReadFile(streamMain);
        IEnumerable<GeometryPoint> dataDiff = ReadFile(streamDiff);
        var listAbolish = ReadList(streamAbolish).ToList();

        return dataMain.Concat(dataDiff).Where(station => !listAbolish.Contains(station.Name));
    }

    /// <summary>
    /// オープンデータの道の駅情報の書式に従ったjsonファイルを解析します。
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException"></exception>
    /// <exception cref="FormatException"></exception>
    public static IEnumerable<GeometryPoint> ReadFile(Stream stream)
    {
        var json = JsonDocument.Parse(stream);
        IEnumerable<GeometryPoint> data = json.RootElement.GetProperty("features").EnumerateArray().Select(obj =>
        {
            JsonElement prop = obj.GetProperty("properties");
            JsonElement[] geo = obj.GetProperty("geometry").GetProperty("coordinates").EnumerateArray().ToArray();
            if (geo.Length != 2)
            {
                throw new NotSupportedException("geometry 'point' must has two element.");
            }
            var lat = geo[1].GetDouble();
            var lng = geo[0].GetDouble();
            var name = prop.GetProperty("道の駅名").GetString() ?? throw new FormatException("property '道の駅名' must be not null;");

            return new GeometryPoint(name, lat, lng);
        });

        return data;
    }

    /// <summary>
    /// 独自の廃止駅リストを読み取ります。
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static IEnumerable<string> ReadList(Stream stream)
    {
        var reader = new StreamReader(stream);
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
