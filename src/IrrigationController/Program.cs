using IrrigationController.Adapters;
using IrrigationController.Components;
using IrrigationController.Core;
using IrrigationController.Core.Services;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
        builder.Services.AddRazorComponents().AddInteractiveServerComponents();
        builder.Services.AddLogging(options =>
        {
            options.AddSimpleConsole(c =>
            {
                c.SingleLine = true;
            });
        });

        Config config = builder.Configuration.GetSection("IrrigationController").Get<Config>() ?? throw new Exception("IrrigationController config is missing");
        RegisterServices(builder.Services, config);

        WebApplication app = builder.Build();
        InitializeServices(app.Services, config);

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }

    private static void RegisterServices(IServiceCollection services, Config config)
    {
        services.AddSingleton(config);

        services.AddSingleton<IIrrigationLog, IrrigationLog>();
        services.AddSingleton<IZoneRepository, ZoneRepository>();
        if (config.MockGpio)
        {
            services.AddSingleton<IZones, FakeZones>();
            services.AddSingleton<FakeRainSensor>();
            services.AddSingleton<IRainSensor>(sp => sp.GetRequiredService<FakeRainSensor>());
            services.AddHostedService<DevToolsBackgroundService>();
        }
        else
        {
            services.AddSingleton(new ZonesConfig(config.Zones.Select(x => x.Pin).ToList()));
            services.AddSingleton<IZones, Zones>();
            services.AddSingleton(new RainSensorConfig(config.RainSensorPin, config.RainSensorSamplingInterval));
            services.AddSingleton<IRainSensor, RainSensor>();
            services.AddSingleton(new ShortCircuitSensorConfig(config.ShortCircuitSensorPin));
            services.AddSingleton<ShortCircuitSensor>();
        }

        services.AddSingleton<IWeatherService, FallbackWeatherService>();
        services.AddSingleton<ProgramController>();
        services.AddSingleton<SoilMoistureEstimator>();
        services.AddSingleton(new SoilMoistureEstimatorConfig(config.Zones.Select(x => (x.MaxPrecipitation, x.PrecipitationRate, x.CropCoefficient)).ToList()));
        services.AddSingleton<SunriseCalculator>();
        services.AddSingleton(new SunriseCalculatorConfig(config.Latitude, config.Longitude));
        services.AddSingleton<SunriseScheduler>();
        services.AddSingleton<ZoneController>();
        services.AddSingleton(new ZoneControllerConfig(config.ZoneDelay));

        services.AddSingleton<ResolveShortCircuitUseCase>();
        services.AddSingleton<GetFullLogUseCase>();
        services.AddSingleton<GetProgramStatusUseCase>();
        services.AddSingleton<GetRecentActivityUseCase>();
        services.AddSingleton<GetZoneStatusUseCase>();
        services.AddSingleton<OpenZoneUseCase>();
        services.AddSingleton(new OpenZoneUseCaseConfig(config.ManualZoneDuration));
        services.AddSingleton<RainDetectedEventHandler>();
        services.AddSingleton<RainClearedEventHandler>();
        services.AddSingleton<RunProgramUseCase>();
        services.AddSingleton<ShortCircuitDetectedEventHandler>();
        services.AddSingleton<SkipUseCase>();
        services.AddSingleton<StopUseCase>();
        services.AddSingleton<SunriseEventHandler>();
        services.AddSingleton(new SunriseEventHandlerConfig(config.Zones.Select(x => (x.Enabled, x.MaxPrecipitation, x.PrecipitationRate)).ToList()));
    }

    private static void InitializeServices(IServiceProvider services, Config config)
    {
        Directory.CreateDirectory(config.AppDataPath);
        if (!config.MockGpio)
        {
            ((Zones)services.GetRequiredService<IZones>()).Initialize();
            ((RainSensor)services.GetRequiredService<IRainSensor>()).Initialize();
            services.GetRequiredService<ShortCircuitSensor>().Initialize();
        }

        services.GetRequiredService<SunriseScheduler>().Initialize();
        services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(() =>
        {
            services.GetRequiredService<ProgramController>().Stop(ZoneCloseReason.Shutdown);
        });
    }
}
