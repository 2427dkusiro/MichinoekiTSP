namespace MichinoekiTSP.Data;

/// <summary>
/// 地図上の点を表現します。
/// </summary>
/// <param name="Name">この地点の名前。</param>
/// <param name="Latitude">緯度。</param>
/// <param name="Longitude">経度。</param>
public record struct GeometryPoint(string Name, double Latitude, double Longitude);
