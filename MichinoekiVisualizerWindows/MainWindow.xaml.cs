using MichinoekiTSP.Data;

using Microsoft.Web.WebView2.Wpf;

using System.Reflection;
using System.Text.Json;
using System.Windows;

using Path = System.IO.Path;

namespace MichinoekiTSP.VisualizerWindows;
/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var webView = new WebView2();
        _ = MainGrid.Children.Add(webView);
        mapView = new(webView);
        MainWindowViewModel viewModel = new(mapView);
        DataContext = viewModel;
    }

    private readonly MapView mapView;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await mapView.Initialize(43.06878725573487, 141.35072226585388, 10);
    }
}

public class MapView
{
    private readonly WebView2 webView;

    public MapView(WebView2 webView)
    {
        this.webView = webView;
    }

    public async Task Initialize(double lat, double lng, int zoom)
    {
        var execDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var html = Path.Combine(execDir!, "Index.html");
        webView.Source = new Uri($"file:///{html}");

        while (webView.CoreWebView2 is null)
        {
            await Task.Delay(10);
        }

        TaskCompletionSource completionSource = new();
        webView.CoreWebView2.DOMContentLoaded += async (sender, e) =>
        {
            await webView.CoreWebView2.ExecuteScriptAsync("""ensureInitialized()""");
            await SetView(lat, lng);
            await SetZoom(zoom);
            completionSource.TrySetResult();
        };

        await completionSource.Task;
    }

    public async Task SetView(double lat, double lng)
    {
        _ = await webView.CoreWebView2.ExecuteScriptAsync($"""setView({lat},{lng})""");
    }

    public async Task SetZoom(int level)
    {
        _ = await webView.CoreWebView2.ExecuteScriptAsync($"""setZoom({level})""");
    }

    public async Task AddMarker(double lat, double lng, string name)
    {
        _ = await webView.CoreWebView2.ExecuteScriptAsync($"""addMarker({lat},{lng},"{name}")""");
    }

    private readonly LinkedList<Polyline> polylines = new();

    public async Task AddPolyline(IEnumerable<GeometryPoint> points, string color)
    {
        var arrayString = $"""[{string.Join(',', points.Select(x => $"[{x.Latitude},{x.Longitude}]"))}]""";
        var result = await webView.CoreWebView2.ExecuteScriptAsync($"""addPolyline({arrayString},"{color}")""");
        var id = JsonSerializer.Deserialize<int>(result);
        polylines.AddLast(new Polyline(id, points));
    }

    public async Task RemovePolyline(Polyline polyline)
    {
        var id = polyline.Id;
        await webView.CoreWebView2.ExecuteScriptAsync($"""removePolyline({id})""");
        polylines.Remove(polyline);
    }

    public async Task ClearPolyline()
    {
        foreach (var polyline in polylines.ToArray())
        {
            await RemovePolyline(polyline);
        }
    }
}

public record struct Polyline(int Id, IEnumerable<GeometryPoint> Point);