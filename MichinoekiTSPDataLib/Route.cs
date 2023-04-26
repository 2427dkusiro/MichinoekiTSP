namespace MichinoekiTSP.Data;

using DateTime = System.DateTime;
using System.Text.Encodings.Web;
using System.Text.Json;

public record Route(GeometryPoint From, GeometryPoint To, string Title, int DistanceMeters, TimeSpan Duration, double AverageSpeed, string Polyline, GeometryPoint[] PolylineDecoded)
{
    public string ToJson()
    {
        var jsonObj = new JsonRoute(DateTime.Now, From.Name, To.Name, Title, DistanceMeters, Duration, Polyline);
        JsonSerializerOptions options = new()
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
        return JsonSerializer.Serialize(jsonObj, options);
    }

    public static Route FromJson(string json, ReadOnlySpan<GeometryPoint> michinoekis)
    {
        var jsonObj = JsonSerializer.Deserialize<JsonRoute>(json) ?? throw new FormatException();
        var average = jsonObj.DistanceMeters / jsonObj.Duration.TotalHours * 1000;
        var decodedPolyline = PolylineEncoder.Decode(jsonObj.Polyline).ToArray();

        GeometryPoint? from = null;
        GeometryPoint? to = null;
        for (int i = 0; i < michinoekis.Length; i++)
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
            jsonObj.Title, jsonObj.DistanceMeters, jsonObj.Duration, average, jsonObj.Polyline, decodedPolyline);
    }
}
