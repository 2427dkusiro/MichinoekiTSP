namespace MichinoekiTSP.CLI;

using MichinoekiTSP.Data;

using System.Reflection;

class Program
{
    static void Main()
    {
        var path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var credential = Path.Combine(path, "google_credential.json");
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credential);

        var data = MichinoekiJsonReader.Read().ToArray();

        var sta1 = data.First(x => x.Name == "三笠");
        var sta2 = data.First(x => x.Name == "スタープラザ 芦別");

        // GoogleRouteAPI.GetRoute(sta1, sta2);
    }
}