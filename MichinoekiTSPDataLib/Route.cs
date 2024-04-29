using System.Text.Encodings.Web;
using System.Text.Json;

using DateTime = System.DateTime;

namespace MichinoekiTSP.Data;

public sealed class Route(GeometryPoint from, GeometryPoint to, string title, int distanceMeters, TimeSpan duration, double averageSpeed, string polyline)
{
    public GeometryPoint From { get; } = from;

    public GeometryPoint To { get; } = to;

    public string Title { get; } = title;

    public int DistanceMeters { get; } = distanceMeters;

    public TimeSpan Duration { get; } = duration;

    public double AverageSpeed { get; } = averageSpeed;

    public string Polyline { get; } = polyline;

    public GeometryPoint[] PolylineDecoded { get => PolylineEncoder.Decode(Polyline).ToArray(); }

    public string ToJson()
    {
        var jsonObj = new JsonRoute(DateTime.Now, From.Name, To.Name, Title, DistanceMeters, Duration, Polyline);
        JsonSerializerOptions options = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        return JsonSerializer.Serialize(jsonObj, options);
    }

    public static Route FromJsonObject(JsonRoute jsonObj, ReadOnlySpan<GeometryPoint> michinoekis)
    {
        var average = jsonObj.DistanceMeters / jsonObj.Duration.TotalHours * 1000;

        GeometryPoint? from = null;
        GeometryPoint? to = null;
        for (var i = 0; i < michinoekis.Length; i++)
        {
            if (michinoekis[i].Name == jsonObj.From)
            {
                from = michinoekis[i];
            }
            if (michinoekis[i].Name == jsonObj.To)
            {
                to = michinoekis[i];
            }
        }

        return new Route(
            from ?? throw new FormatException($"point in json '{jsonObj.From}' was not found"),
            to ?? throw new FormatException($"point in json '{jsonObj.From}' was not found"),
            jsonObj.Title, jsonObj.DistanceMeters, jsonObj.Duration, average, jsonObj.Polyline);
    }

    public static Route FromJson(string json, ReadOnlySpan<GeometryPoint> michinoekis)
    {
        JsonRoute jsonObj = JsonSerializer.Deserialize<JsonRoute>(json) ?? throw new FormatException();
        return FromJsonObject(jsonObj, michinoekis);
    }

    public static Route FromJson(Stream stream, ReadOnlySpan<GeometryPoint> michinoekis)
    {
        JsonRoute jsonObj = JsonSerializer.Deserialize<JsonRoute>(stream) ?? throw new FormatException();
        return FromJsonObject(jsonObj, michinoekis);
    }

    private int? _hashCode = null;

    public override int GetHashCode()
    {
        return _hashCode ??= HashCode.Combine(From.Latitude, From.Longitude, To.Latitude, To.Longitude);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Route route)
        {
            return base.Equals(obj);
        }
        return From == route.From && To == route.To;
    }
}
