using System.Globalization;
using System.Threading.Tasks;
using Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Server.Commands;
using WDLT.Frameworks.Telegram;
using WDLT.Frameworks.Telegram.Models;

namespace Server
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public async void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();

            services.AddDbContext<AppDatabase>(options => 
                options.UseSqlServer(Configuration.GetConnectionString("Local")).UseLazyLoadingProxies());

            var settings = new InitSettings();
            var settingSection = Configuration.GetSection("Bot");
            settingSection.Bind(settings);

            var framework = new TelegramFramework(settings);
            services.AddSingleton(framework);
            services.AddSingleton<ITelegramCommand, SubscribeCommand>();
            services.AddSingleton<ITelegramCommand, UnsubscribeCommand>();
        }

        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var cultureInfo = new CultureInfo("en-US");

            CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
            CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            await InitTelegram(app);
        }

        private async Task InitTelegram(IApplicationBuilder app)
        {
            using var provider = app.ApplicationServices.CreateScope();

            var telegram = provider.ServiceProvider.GetRequiredService<TelegramFramework>();

            foreach (var command in provider.ServiceProvider.GetServices<ITelegramCommand>())
            {
                telegram.AddCommand(command);
            }

            await telegram.SetWebHookAsync();
            await telegram.SetBotCommands();
        }
    }
}
