using IrrigationController.Adapters;
using IrrigationController.Components;
using IrrigationController.Core;
using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
using IrrigationController.Core.Infrastructure;

namespace IrrigationController
{
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
            services.AddSingleton<IValveRepository, ValveRepository>();
            if (config.MockGpio)
            {
                services.AddSingleton<IValves, FakeValves>();
                services.AddSingleton<FakeRainSensor>();
                services.AddSingleton<IRainSensor>(sp => sp.GetRequiredService<FakeRainSensor>());
                services.AddHostedService<DevToolsBackgroundService>();
            }
            else
            {
                services.AddSingleton(new ValvesConfig(config.Valves.Select(x => x.Pin).ToList()));
                services.AddSingleton<IValves, Valves>();
                services.AddSingleton(new RainSensorConfig(config.RainSensorPin, config.RainSensorSamplingInterval));
                services.AddSingleton<IRainSensor, RainSensor>();
                services.AddSingleton(new ShortCircuitSensorConfig(config.ShortCircuitSensorPin));
                services.AddSingleton<ShortCircuitSensor>();
            }

            services.AddSingleton<ProgramController>();
            services.AddSingleton<SunriseCalculator>();
            services.AddSingleton(new SunriseCalculatorConfig(config.Latitude, config.Longitude));
            services.AddSingleton<SunriseScheduler>();
            services.AddSingleton<ValveController>();
            services.AddSingleton(new ValveControllerConfig(config.ValveDelay));

            services.AddSingleton<FixValveUseCase>();
            services.AddSingleton<GetFullLogUseCase>();
            services.AddSingleton<GetProgramStatusUseCase>();
            services.AddSingleton<GetRecentActivityUseCase>();
            services.AddSingleton<GetValveStatusUseCase>();
            services.AddSingleton<OpenValveUseCase>();
            services.AddSingleton(new OpenValveUseCaseConfig(config.ManualLimit));
            services.AddSingleton<RainDetectedEventHandler>();
            services.AddSingleton<RainClearedEventHandler>();
            services.AddSingleton<RunProgramUseCase>();
            services.AddSingleton<ShortCircuitDetectedEventHandler>();
            services.AddSingleton<SkipUseCase>();
            services.AddSingleton<StopUseCase>();
            services.AddSingleton<SunriseEventHandler>();
        }

        private static void InitializeServices(IServiceProvider services, Config config)
        {
            Directory.CreateDirectory(config.AppDataPath);
            if (!config.MockGpio)
            {
                ((Valves)services.GetRequiredService<IValves>()).Initialize();
                ((RainSensor)services.GetRequiredService<IRainSensor>()).Initialize();
                services.GetRequiredService<ShortCircuitSensor>().Initialize();
            }

            services.GetRequiredService<SunriseScheduler>().Initialize();
            services.GetRequiredService<IHostApplicationLifetime>().ApplicationStopping.Register(() =>
            {
                services.GetRequiredService<ProgramController>().Stop(IrrigationStopReason.Manual);
            });
        }
    }
}
