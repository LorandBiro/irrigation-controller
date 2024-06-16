using IrrigationController.Adapters;
using IrrigationController.Components;
using IrrigationController.Core;
using IrrigationController.Core.Controllers;
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
                services.AddSingleton<IRainSensor, FakeRainSensor>();
                services.AddHostedService<GpioSimulatorBackgroundService>();
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
            services.AddSingleton<ValveController>();
            services.AddSingleton(new ValveControllerConfig(config.ValveDelay));

            services.AddSingleton<FixValveUseCase>();
            services.AddSingleton<GetFullLogUseCase>();
            services.AddSingleton<GetProgramStatusUseCase>();
            services.AddSingleton<GetRecentActivityUseCase>();
            services.AddSingleton<GetValveStatusUseCase>();
            services.AddSingleton<OpenValveUseCase>();
            services.AddSingleton(new OpenValveUseCaseConfig(config.ManualLimit));
            services.AddSingleton<ProgramStepCompletedEventHandler>();
            services.AddSingleton<RunProgramUseCase>();
            services.AddSingleton<ShortCircuitDetectedEventHandler>();
            services.AddSingleton<SkipUseCase>();
            services.AddSingleton<StopUseCase>();
        }

        private static void InitializeServices(IServiceProvider services, Config config)
        {
            if (!config.MockGpio)
            {
                ((Valves)services.GetRequiredService<IValves>()).Init();
                ((RainSensor)services.GetRequiredService<IRainSensor>()).Init();
                services.GetRequiredService<ShortCircuitSensor>()?.Init();
            }
        }
    }
}
