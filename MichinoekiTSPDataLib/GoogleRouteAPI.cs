using Google.Api.Gax;
using Google.Api.Gax.Grpc;
using Google.Maps.Routing.V2;
using Google.Type;

using System.Reflection;

namespace MichinoekiTSP.Data;
/// <summary>
/// Google Maps の Route API を利用する機能を提供します。
/// </summary>
public class GoogleRouteAPIClient
{
    private static readonly int maxRetry = 3;

    /// <summary>
    /// <see cref="GoogleRouteAPIClient"/> クラスの新しいインスタンスを初期化します。
    /// </summary>
    public GoogleRouteAPIClient()
    {
        var execPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var credential = Path.Combine(execPath!, "google_credential.json");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential);
    }

    /// <summary>
    /// 道の駅間のルートを、デフォルトの設定で取得します。
    /// </summary>
    /// <param name="from">出発点。</param>
    /// <param name="to">到着点</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<Route> GetRoute(GeometryPoint from, GeometryPoint to, Action<string>? writeLog = null)
    {
        RoutesClient _client = await RoutesClient.CreateAsync();
        var callSettings = CallSettings.FromHeader("X-Goog-FieldMask", "*");
        var request = new ComputeRoutesRequest
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

        ComputeRoutesResponse? response = null;
        int retry = 0;

        async Task TryGet()
        {
            try
            {
                response = await _client.ComputeRoutesAsync(request, callSettings) ?? throw new InvalidOperationException("API response was null.");
                if (!response.Routes.Any())
                {
                    throw new InvalidOperationException("API returned an invalid response.");
                }
            }
            catch (Exception ex)
            {
                writeLog?.Invoke($"API Error Response:{ex}");
                if (retry++ < maxRetry)
                {
                    await Task.Delay(1000);
                    _client = await RoutesClient.CreateAsync();
                    await TryGet();
                }
                else
                {
                    throw;
                }
            }
        }
        await TryGet();

        if (response is null)
        {
            throw new InvalidOperationException();
        }

        Google.Maps.Routing.V2.Route route = response.Routes.First();
        var title = route.Description;
        var distance = route.DistanceMeters;
        var duration = TimeSpan.FromSeconds(route.Duration.Seconds + (route.Duration.Nanos / 1_000_000_000));
        var averageSpeed = distance / duration.TotalHours * 1000;
        var polyline = route.Polyline.EncodedPolyline;

        var obj = new Route(from, to, title, distance, duration, averageSpeed, polyline);
        return obj;
    }
}
