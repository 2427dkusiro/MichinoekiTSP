namespace MichinoekiTSP.Data;

using Google.Api.Gax.Grpc;
using Google.Maps.Routing.V2;
using Google.Type;

using DateTime = System.DateTime;
using System.Text.Encodings.Web;
using System.Text.Json;

public class GoogleRouteAPI
{
    public static Route GetRoute(MichinoekiGeometry from, MichinoekiGeometry to)
    {
        RoutesClient client = RoutesClient.Create();
        CallSettings callSettings = CallSettings.FromHeader("X-Goog-FieldMask", "*");
        ComputeRoutesRequest request = new ComputeRoutesRequest
        {
            Origin = new Waypoint
            {
                Location = new Location { LatLng = new LatLng { Latitude = from.Latitude, Longitude = from.Longitude } }
            },
            Destination = new Waypoint
            {
                Location = new Location { LatLng = new LatLng { Latitude = to.Latitude, Longitude = to.Longitude } }
            },
            TravelMode = RouteTravelMode.Drive,
            RoutingPreference = RoutingPreference.TrafficUnaware,
            PolylineQuality = PolylineQuality.HighQuality,
            RouteModifiers = new RouteModifiers()
            {
                AvoidFerries = true,
                AvoidTolls = true
            }
        };

        ComputeRoutesResponse response = client.ComputeRoutes(request, callSettings) ?? throw new InvalidOperationException("API response was null.");
        if (!response.Routes.Any())
        {
            throw new InvalidOperationException("API returned an invalid response.");
        }

        var route = response.Routes.First();
        var title = route.Description;
        int distance = route.DistanceMeters;
        var duration = TimeSpan.FromSeconds(route.Duration.Seconds + route.Duration.Nanos / 1_000_000_000);
        var averageSpeed = distance / duration.TotalHours * 1000;
        var polyline = route.Polyline.EncodedPolyline;
        var decodedPolyline = PolylineEncoder.Decode(polyline).ToArray();

        var obj = new Route(from, to, title, distance, duration, averageSpeed, polyline, decodedPolyline);
        return obj;
    }
}

public record Route(MichinoekiGeometry From, MichinoekiGeometry To, string Title, int DistanceMeters, TimeSpan Duration, double AverageSpeed, string Polyline, GeometryPoint[] PolylineDecoded)
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

    public static Route FromJson(string json, MichinoekiGeometry[] michinoekis)
    {
        var jsonObj = JsonSerializer.Deserialize<JsonRoute>(json) ?? throw new FormatException();
        var average = jsonObj.DistanceMeters / jsonObj.Duration.TotalHours * 1000;
        var decodedPolyline = PolylineEncoder.Decode(jsonObj.Polyline).ToArray();

        var from = michinoekis.FirstOrDefault(x => x.Name == jsonObj.From) ?? throw new InvalidOperationException($"name '{jsonObj.From}' was not found.");
        var to = michinoekis.FirstOrDefault(x => x.Name == jsonObj.To) ?? throw new InvalidOperationException($"name '{jsonObj.From}' was not found.");
        return new Route(from, to, jsonObj.Title, jsonObj.DistanceMeters, jsonObj.Duration, average, jsonObj.Polyline, decodedPolyline);
    }
}

public record JsonRoute(DateTime CreateTimeStamp, string From, string To, string Title, int DistanceMeters, TimeSpan Duration, string Polyline);