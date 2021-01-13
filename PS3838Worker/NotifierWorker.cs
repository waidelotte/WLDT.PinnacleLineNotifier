using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Database;
using Database.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WDLT.Frameworks.Telegram;
using WDLT.Workers.Base;

namespace PS3838Worker
{
    public class NotifierWorker : BaseBackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TelegramFramework _framework;

        public NotifierWorker(IServiceProvider serviceProvider, TelegramFramework framework)
        {
            Timeout = TimeSpan.FromSeconds(30);
            _serviceProvider = serviceProvider;
            _framework = framework;
        }

        protected override async Task DoWorkAsync(CancellationToken ct)
        {
            await using var db = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<AppDatabase>();
            
            var toNotify = await db.Notifies.Where(w => !w.IsExecuted).ToListAsync(ct);

            if (toNotify.Any())
            {
                foreach (var notify in toNotify)
                {
                    var message = new TelegramMessageBuilder();

                    message.AddLine("😮 <b>Bets are Open!</b>");
                    message.AddLine();
                    message.AddLine($"⚽️ {notify.Event.League.Name}");
                    message.AddLine($"  ∟ ID:{notify.Event.Id}");
                    message.AddLine($"  ∟ {notify.Event.Home} - {notify.Event.Away}");

                    var startAfter = notify.Event.StartAt - DateTimeOffset.Now;
                    var time = startAfter >= TimeSpan.FromDays(1) ? $"{startAfter.Days} Days and {startAfter.Hours} Hours" : $"{startAfter.Hours} Hours and {startAfter.Minutes}m.";
                    message.AddLine($"  ∟ <b>Start After:</b> {time}");

                    var moneyline = notify.Event.Lines.FirstOrDefault(f => f.Type == EDBLineType.Open && f.Line == EDBLine.Moneyline);

                    if (moneyline != null)
                    {
                        message.AddLine();
                        message.AddLine("📈 <b>1x2</b>");
                        message.AddLine($"  ∟ <b>{moneyline.T1:F2} {moneyline.T2:F2} {moneyline.T3:F2}</b>");
                        message.AddLine($"  ∟ Max Bet <b>{moneyline.WinMaxRiskStake:F0}</b>");
                    }

                    foreach (var subscribe in await db.Subscribes.ToListAsync(ct))
                    {
                        await _framework.SendTextAsync(message, subscribe.UserId);
                        await Task.Delay(TimeSpan.FromSeconds(1), ct);
                    }

                    notify.IsExecuted = true;
                }

                await db.SaveChangesAsync(ct);
            }
        }
    }
}