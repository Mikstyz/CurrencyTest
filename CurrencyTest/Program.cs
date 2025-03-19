using Serilog;

namespace Prog
{
    public class Program
    {
        public async static Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .MinimumLevel.Information()
                .CreateLogger();


            /*try
            {
                if (BitffinexApi.StatPlatform())
                {
                    Log.Information("Api сервер доступен, запуск проекта");

                    var builder = WebApplication.CreateBuilder(args);

                    builder.Services.AddRazorPages();

                    var app = builder.Build();

                    if (!app.Environment.IsDevelopment())
                    {
                        app.UseExceptionHandler("/Error");
                    }

                    app.UseStaticFiles();

                    app.UseRouting();

                    app.UseAuthorization();

                    app.MapRazorPages();

                    app.Run();
                }

                Log.Warning("В данный момент api сервис не доступен");
            }

            catch (Exception ex)
            {
                Log.Information($"Ошибка при запуске проекта:\n{ex}");
            }*/
        }

    }
}


//MikstyApiKeyTestAppApiKey
//93d5ae3d96a2ff9da78560537b356a9cc9c84ff7441ae51e77223ea3ac121e78
