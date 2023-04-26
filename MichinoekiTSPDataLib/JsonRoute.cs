
using DateTime = System.DateTime;

namespace MichinoekiTSP.Data;
public record JsonRoute(DateTime CreateTimeStamp, string From, string To, string Title, int DistanceMeters, TimeSpan Duration, string Polyline);