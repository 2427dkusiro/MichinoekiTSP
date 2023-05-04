using DateTime = System.DateTime;

namespace MichinoekiTSP.Data;

/// <summary>
/// Json にシリアライズする際の経路表現を表します。
/// </summary>
/// <param name="CreateTimeStamp">作成した時刻。</param>
/// <param name="From">出発点の名前。</param>
/// <param name="To">目的地の名前。</param>
/// <param name="Title">経路のタイトル。</param>
/// <param name="DistanceMeters">経路の距離(メートル単位)。</param>
/// <param name="Duration">経路の所要時間。</param>
/// <param name="Polyline">経路を表す、google 形式でエンコードされたポリライン。</param>
public record JsonRoute(DateTime CreateTimeStamp, string From, string To, string Title, int DistanceMeters, TimeSpan Duration, string Polyline);