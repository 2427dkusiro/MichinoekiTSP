﻿using MichinoekiTSP.Data;
using MichinoekiTSP.Data.Solvers;

using System.Windows.Input;

namespace MichinoekiTSP.VisualizerWindows;
public class MainWindowViewModel : ViewModelBase
{
    private readonly MapView mapView;

    private MichinoekiResourceManager? _manager;

    public MichinoekiResourceManager? Manager { get => _manager; private set => SetProperty(ref _manager, value); }

    public MainWindowViewModel(MapView mapView)
    {
        this.mapView = mapView;
    }

    private bool _resourceLoading = false;

    public bool ResourceLoading
    {
        get => _resourceLoading;
        private set => SetProperty(ref _resourceLoading, value);
    }

    private DelegateCommand? loadResourceCommand;
    public ICommand LoadResourceCommand
    {
        get => loadResourceCommand ??= RegisterCommand(new DelegateCommand()
        {
            ExecuteHandler = async _ =>
            {
                ResourceLoading = true;
                MichinoekiResourceManager? m = null;
                await Task.Run(() =>
                {
                    m = MichinoekiResourceManager.CreateInstance();
                });
                Manager = m;

                foreach (GeometryPoint point in _manager!.Michinoekis)
                {
                    await mapView.AddMarker(point.Latitude, point.Longitude, point.Name);
                }
            },
            CanExecuteHandler = _ => !ResourceLoading
        }, nameof(ResourceLoading));
    }

    private DelegateCommand? calcCommand;
    public ICommand CalcCommand
    {
        get => calcCommand ??= RegisterCommand(new DelegateCommand()
        {
            ExecuteHandler = async _ =>
            {
                await mapView.ClearPolyline();
                var start = _manager!.Michinoekis.First(x => x.Name == "三笠");
                var solver = new TSPSolverBuilder()
                    .AddParameter(new TSPSolverContext(_manager!, start, new Random()))
                    .UseInitialSolver<NearestNeighbor>()
                    //.UseOptimizer<TwoOptLS>(new TwoOptLSParameter(100))
                    //.UseOptimizer<DoubleBridgeOptimizer>()
                    //.UseExecuter<SimpleLoopExecuter>(new SimpleLoopParameter(10))
                    .UseExecuter<TwoOptSA>(new TwoOptSAParameter(2_000_000, 8000, 10))
                    .UseExecuter<RepeatExecuter>(new RepeatExecuterParameter(30))
                    .Build();

                TSPAnswer? answer = null;
                await Task.Run(() =>
                {
                    answer = solver();
                });

                TSPAnswer = answer!;

                for (int i = 0; i < answer!.Routes.Length; i++)
                {
                    await mapView.AddPolyline(answer!.Routes[i].PolylineDecoded, "#0000bb");
                }
            },
            CanExecuteHandler = _ => _manager is not null
        }, nameof(Manager));
    }

    private TSPAnswer? tspAnswer;

    public TSPAnswer? TSPAnswer
    {
        get => tspAnswer;
        set
        {
            SetProperty(ref tspAnswer, value);
            RaisePropertyChanged(nameof(RouteDuration));
            RaisePropertyChanged(nameof(RouteDistance));
        }
    }

    public string? RouteDuration
    {
        get => tspAnswer?.TotalTime.ToString("d'日'hh'時間'mm'分'ss'秒'");
    }

    public string? RouteDistance
    {
        get => tspAnswer is null ? null : $"{tspAnswer.TotalDistance / 1000m}km";
    }
}
