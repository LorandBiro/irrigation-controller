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
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            ValveConfig valveConfig = builder.Configuration.GetSection("ValveConfig").Get<ValveConfig>() ?? throw new Exception("ValveConfig is missing");
            builder.Services.AddSingleton(valveConfig);

            if (builder.Configuration.GetValue<bool>("MockGpio"))
            {
                builder.Services.AddSingleton<IGpio, FakeGpio>();
            }
            else
            {
                builder.Services.AddSingleton<IGpio, Gpio>();
            }

            builder.Services.AddSingleton<ValveController>();
            builder.Services.AddSingleton<ProgramController>();
            builder.Services.AddSingleton<OpenValveUseCase>();
            builder.Services.AddSingleton<CloseValveUseCase>();
            builder.Services.AddSingleton<GetValveStatusUseCase>();
            builder.Services.AddSingleton<RunProgramUseCase>();

            var app = builder.Build();

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
