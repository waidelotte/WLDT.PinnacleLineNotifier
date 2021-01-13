using Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using WDLT.Clients.PS3838;
using WDLT.Frameworks.Telegram;
using WDLT.Frameworks.Telegram.Models;

namespace PS3838Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    services.AddOptions();

                    services.AddDbContext<AppDatabase>(options =>
                        options.UseSqlServer(context.Configuration.GetConnectionString("Local")).UseLazyLoadingProxies());

                    var config = context.Configuration;

                    services.Configure<PS3838Settings>(config.GetSection("PS3838"));

                    var tgSettings = new InitSettings();
                    var tgSettingSection = config.GetSection("Bot");
                    tgSettingSection.Bind(tgSettings);

                    var psSettings = new PS3838Settings();
                    var psSettingSection = config.GetSection("PS3838");
                    psSettingSection.Bind(psSettings);

                    var framework = new TelegramFramework(tgSettings);
                    services.AddSingleton(framework);

                    var ps3838Client = new PS3838Client(psSettings.UserAgent);
                    ps3838Client.Auth(psSettings.Login, psSettings.Password);
                    services.AddSingleton(ps3838Client);

                    services.AddHostedService<NotifierWorker>();
                    services.AddHostedService<DataWorker>();
                });
    }
}
