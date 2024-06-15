using IrrigationController.Adapters;
using IrrigationController.Components;
using IrrigationController.Core.Controllers;
using IrrigationController.Core.Domain;
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

            builder.Services.AddSingleton(builder.Configuration.GetSection("RainSensor").Get<RainSensorConfig>() ?? throw new Exception("RainSensor config is missing"));
            builder.Services.AddSingleton(builder.Configuration.GetSection("ShortCircuitSensor").Get<ShortCircuitSensorConfig>() ?? throw new Exception("ShortCircuitSensor config is missing"));
            builder.Services.AddSingleton(builder.Configuration.GetSection("ValveConfig").Get<ValveConfig>() ?? throw new Exception("ValveConfig is missing"));
            if (builder.Configuration.GetValue<bool>("MockGpio"))
            {
                builder.Services.AddSingleton<IGpio, FakeGpio>();
                builder.Services.AddSingleton<IRainSensor, FakeRainSensor>();
            }
            else
            {
                builder.Services.AddSingleton<IGpio, Gpio>();
                builder.Services.AddSingleton<IRainSensor, RainSensor>();
                builder.Services.AddSingleton<ShortCircuitSensor>();
            }

            builder.Services.AddSingleton<ValveController>();
            builder.Services.AddSingleton<ProgramController>();
            builder.Services.AddSingleton<OpenValveUseCase>();
            builder.Services.AddSingleton<StopUseCase>();
            builder.Services.AddSingleton<SkipUseCase>();
            builder.Services.AddSingleton<GetValveStatusUseCase>();
            builder.Services.AddSingleton<RunProgramUseCase>();
            builder.Services.AddSingleton<GetProgramStatusUseCase>();
            builder.Services.AddSingleton<ShortCircuitDetectedEventHandler>();

            WebApplication app = builder.Build();
            app.Services.GetRequiredService<ValveController>().Init();
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
