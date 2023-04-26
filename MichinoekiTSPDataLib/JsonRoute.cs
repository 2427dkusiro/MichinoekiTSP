namespace MichinoekiTSP.Data;

using DateTime = System.DateTime;

public record JsonRoute(DateTime CreateTimeStamp, string From, string To, string Title, int DistanceMeters, TimeSpan Duration, string Polyline);