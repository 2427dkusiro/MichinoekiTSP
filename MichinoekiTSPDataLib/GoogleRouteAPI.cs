namespace MichinoekiTSP.Data;

using Google.Api.Gax.Grpc;
using Google.Maps.Routing.V2;
using Google.Type;

using System.Reflection;

/// <summary>
/// Google Maps の Route API を利用する機能を提供します。
/// </summary>
public class GoogleRouteAPIClient
{
    private readonly RoutesClient _client;

    /// <summary>
    /// <see cref="GoogleRouteAPIClient"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    public GoogleRouteAPIClient()
    {
        var execPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var credential = Path.Combine(execPath!, "google_credential.json");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential);
        _client = RoutesClient.Create();
    }

    /// <summary>
    /// 道の駅間のルートを、デフォルトの設定で取得します。
    /// </summary>
    /// <param name="from">出発点。</param>
    /// <param name="to">到着点</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Route> GetRoute(GeometryPoint from, GeometryPoint to)
    {
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

        ComputeRoutesResponse response = await _client.ComputeRoutesAsync(request, callSettings) ?? throw new InvalidOperationException("API response was null.");
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
