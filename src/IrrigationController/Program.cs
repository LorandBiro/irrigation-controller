using IrrigationController.Adapters;
using IrrigationController.Components;
using IrrigationController.Core;

namespace IrrigationController
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            List<ValveConfig> valves = builder.Configuration.GetSection("Valves").Get<List<ValveConfig>>() ?? throw new Exception("Valves configuration is missing");
            builder.Services.AddSingleton<IReadOnlyList<ValveConfig>>(valves);

            bool mockGpio = builder.Configuration.GetValue<bool>("MockGpio");
            if (mockGpio)
            {
                builder.Services.AddSingleton<IValveController, FakeValveController>();
            }
            else
            {
                builder.Services.AddSingleton<IValveController, GpioValveController>();
            }

            builder.Services.AddSingleton<OpenValveUseCase>();
            builder.Services.AddSingleton<CloseValveUseCase>();
            builder.Services.AddSingleton<GetValveStatusUseCase>();

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
