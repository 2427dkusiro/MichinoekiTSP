namespace MichinoekiTSP.Data;

using Google.Api.Gax.Grpc;
using Google.Maps.Routing.V2;
using Google.Type;

public class GoogleRouteAPI
{
    public static void GetRoute(MichinoekiGeometry from, MichinoekiGeometry to)
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

        return;
    }
}
