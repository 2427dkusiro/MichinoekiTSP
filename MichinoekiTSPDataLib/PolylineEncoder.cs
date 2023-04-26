using System.Text;

namespace MichinoekiTSP.Data;

/// <summary>
/// See https://developers.google.com/maps/documentation/utilities/polylinealgorithm
/// </summary>
/// <remarks>
/// Code from https://gist.github.com/shinyzhu/4617989
/// </remarks>
public static class PolylineEncoder
{
    /// <summary>
    /// Decode google style polyline coordinates.
    /// </summary>
    /// <param name="encodedPoints"></param>
    /// <returns></returns>
    public static IEnumerable<GeometryPoint> Decode(string encodedPoints)
    {
        ArgumentNullException.ThrowIfNull(encodedPoints);

        var polylineChars = encodedPoints;
        var index = 0;

        var currentLat = 0;
        var currentLng = 0;
        int next5bits;
        int sum;
        int shifter;

        while (index < polylineChars.Length)
        {
            // calculate next latitude
            sum = 0;
            shifter = 0;
            do
            {
                next5bits = polylineChars[index++] - 63;
                sum |= (next5bits & 31) << shifter;
                shifter += 5;
            } while (next5bits >= 32 && index < polylineChars.Length);

            if (index >= polylineChars.Length)
            {
                break;
            }

            currentLat += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

            //calculate next longitude
            sum = 0;
            shifter = 0;
            do
            {
                next5bits = polylineChars[index++] - 63;
                sum |= (next5bits & 31) << shifter;
                shifter += 5;
            } while (next5bits >= 32 && index < polylineChars.Length);

            if (index >= polylineChars.Length && next5bits >= 32)
            {
                break;
            }

            currentLng += (sum & 1) == 1 ? ~(sum >> 1) : (sum >> 1);

            yield return new GeometryPoint
            {
                Latitude = Convert.ToDouble(currentLat) / 1E5,
                Longitude = Convert.ToDouble(currentLng) / 1E5
            };
        }
    }

    /// <summary>
    /// Encode it
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    public static string Encode(IEnumerable<GeometryPoint> points)
    {
        var str = new StringBuilder();

        void encodeDiff(int diff)
        {
            var shifted = diff << 1;
            if (diff < 0)
            {
                shifted = ~shifted;
            }

            var rem = shifted;

            while (rem >= 0x20)
            {
                _ = str.Append((char)((0x20 | (rem & 0x1f)) + 63));

                rem >>= 5;
            }

            _ = str.Append((char)(rem + 63));
        }

        var lastLat = 0;
        var lastLng = 0;

        foreach (GeometryPoint point in points)
        {
            var lat = (int)Math.Round(point.Latitude * 1E5);
            var lng = (int)Math.Round(point.Longitude * 1E5);

            encodeDiff(lat - lastLat);
            encodeDiff(lng - lastLng);

            lastLat = lat;
            lastLng = lng;
        }

        return str.ToString();
    }
}
