using System;
using System.Threading.Tasks;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
namespace Server.Commands
{
    public class UnsubscribeCommand : BaseServerCommand
    {
        public UnsubscribeCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            IsPublic = false;
            IsVisible = true;
            Triggers.Add("unsubscribe");
            Description = "Unsubscribe from Notifications";
        }

        protected override async Task ExecuteCommandAsync(Message message, string textWithoutCommand)
        {
            await using var db = CreateScope();

            var existSubscribe = await db.Subscribes.FirstOrDefaultAsync(f => f.UserId == message.From.Id);

            if (existSubscribe != null)
            {
                db.Subscribes.Remove(existSubscribe);
                await db.SaveChangesAsync();
                await Framework.SendTextAsync("You have successfully unsubscribed!", message.Chat);
            }
        }
    }
}