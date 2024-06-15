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

            builder.Services.AddSingleton(builder.Configuration.GetSection("ValveConfig").Get<ValveConfig>() ?? throw new Exception("ValveConfig is missing"));
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
            builder.Services.AddSingleton<GetProgramStatusUseCase>();

            WebApplication app = builder.Build();
            app.Services.GetRequiredService<ValveController>().Init();

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
