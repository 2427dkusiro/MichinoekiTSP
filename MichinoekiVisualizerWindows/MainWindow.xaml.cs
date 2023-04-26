using MichinoekiTSP.Data;

using Microsoft.Web.WebView2.Wpf;

using System.Diagnostics;
using System.Reflection;
using System.Windows;

using Path = System.IO.Path;

namespace MichinoekiVisualizerWindows;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private WebView2 webView;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var execDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var html = Path.Combine(execDir, "Index.html");

        webView = new WebView2()
        {
            Source = new Uri($"file:///{html}")
        };
        _ = MainGrid.Children.Add(webView);

        webView.ContentLoading += async (sender, e) =>
        {
            await InitializeMap();
            await RenderAnswer();
        };
    }

    private async Task SetView(double lat, double lng)
    {
        _ = await webView.CoreWebView2.ExecuteScriptAsync($"""setView({lat},{lng})""");
    }

    private async Task SetZoom(int level)
    {
        _ = await webView.CoreWebView2.ExecuteScriptAsync($"""setZoom({level})""");
    }

    private async Task AddMarker(double lat, double lng, string name)
    {
        _ = await webView.CoreWebView2.ExecuteScriptAsync($"""addMarker({lat},{lng},"{name}")""");
    }

    private async Task AddPolyline(IEnumerable<GeometryPoint> points, string color)
    {
        var arrayString = $"""[{string.Join(',', points.Select(x => $"[{x.Latitude},{x.Longitude}]"))}]""";
        var result = await webView.CoreWebView2.ExecuteScriptAsync($"""addPolyline({arrayString},"{color}")""");
    }

    private async Task InitializeMap()
    {
        // TODO:もうちょっとちゃんと待つ
        await Task.Delay(100);
        await SetView(43.06878725573487, 141.35072226585388);
        await Task.Delay(100);
        await SetZoom(10);
    }

    // ひとまずデバッグ用
    private async Task RenderAnswer()
    {
        MichinoekiResourceManager? manager = null;
        await Task.Run(() =>
        {
            manager = MichinoekiResourceManager.CreateInstance();
        });

        foreach (GeometryPoint point in manager.Michinoekis)
        {
            await AddMarker(point.Latitude, point.Longitude, point.Name);
        }

        await Task.Delay(100);

        var start = manager.Michinoekis.First(x => x.Name == "三笠");
        var solver = new TSPSolver(manager, start);
        var answer = solver.TwoOptILS();

        Debug.WriteLine($"時間:{answer.TotalTime}");
        Debug.WriteLine($"距離:{answer.TotalDistance / 1000m}km");

        foreach (Route route in answer.Routes)
        {
            await AddPolyline(route.PolylineDecoded, "#0000bb");
        }
    }
}