using IrrigationController.Adapters;
using IrrigationController.Components;
using IrrigationController.Core.Controllers;
using IrrigationController.Core.Infrastructure;
using IrrigationController.Core.UseCases;

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
            if (!Directory.Exists(config.AppDataPath))
            {
                Directory.CreateDirectory(config.AppDataPath);
            }

            builder.Services.AddSingleton(config);
            if (builder.Configuration.GetValue<bool>("MockGpio"))
            {
                builder.Services.AddSingleton<IValves, FakeValves>();
                builder.Services.AddSingleton<IRainSensor, FakeRainSensor>();
                builder.Services.AddHostedService<GpioSimulatorBackgroundService>();
            }
            else
            {
                builder.Services.AddSingleton(new ValvesConfig(config.Valves.Select(x => x.Pin).ToList()));
                builder.Services.AddSingleton<IValves, Valves>();
                builder.Services.AddSingleton(new RainSensorConfig(config.RainSensorPin, config.RainSensorSamplingInterval));
                builder.Services.AddSingleton<IRainSensor, RainSensor>();
                builder.Services.AddSingleton(new ShortCircuitSensorConfig(config.ShortCircuitSensorPin));
                builder.Services.AddSingleton<ShortCircuitSensor>();
            }

            builder.Services.AddSingleton(new ValveControllerConfig(config.ValveDelay));
            builder.Services.AddSingleton<ValveController>();
            builder.Services.AddSingleton<ProgramController>();
            builder.Services.AddSingleton(new OpenValveUseCaseConfig(config.ManualLimit));
            builder.Services.AddSingleton<OpenValveUseCase>();
            builder.Services.AddSingleton<StopUseCase>();
            builder.Services.AddSingleton<SkipUseCase>();
            builder.Services.AddSingleton<GetValveStatusUseCase>();
            builder.Services.AddSingleton<RunProgramUseCase>();
            builder.Services.AddSingleton<GetProgramStatusUseCase>();
            builder.Services.AddSingleton<ShortCircuitDetectedEventHandler>();
            builder.Services.AddSingleton<IValveRepository>(new ValveRepository(config.AppDataPath));

            WebApplication app = builder.Build();
            (app.Services.GetRequiredService<IValves>() as Valves)?.Init();
            (app.Services.GetRequiredService<IRainSensor>() as RainSensor)?.Init();
            app.Services.GetService<ShortCircuitSensor>()?.Init();

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
    }
}
