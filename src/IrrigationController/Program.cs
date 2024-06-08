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

            int[] valvePins = builder.Configuration.GetSection("ValvePins").Get<int[]>() ?? throw new Exception("ValvePins configuration is missing");
            bool mockGpio = builder.Configuration.GetValue<bool>("MockGpio");
            if (mockGpio)
            {
                builder.Services.AddSingleton<IValveController, FakeValveController>();
            }
            else
            {
                builder.Services.AddSingleton<IValveController>(new GpioValveController(valvePins));
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
