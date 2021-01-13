using System;
using System.Threading.Tasks;
using Database.Entities;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
namespace Server.Commands
{
    public class SubscribeCommand : BaseServerCommand
    {
        public SubscribeCommand(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            IsPublic = false;
            IsVisible = true;
            Triggers.Add("subscribe");
            Triggers.Add("start");
            Description = "Subscribe to Notifications";
        }

        protected override async Task ExecuteCommandAsync(Message message, string textWithoutCommand)
        {
            await using var db = CreateScope();

            var existSubscribe = await db.Subscribes.FirstOrDefaultAsync(f => f.UserId == message.From.Id);

            if (existSubscribe == null)
            {
                await db.Subscribes.AddAsync(new DBSubscribe {UserId = message.From.Id});
                await db.SaveChangesAsync();
                await Framework.SendTextAsync("You have successfully Subscribed!", message.Chat);
            }
            else
            {
                await Framework.SendTextAsync("You are already Subscribed", message.Chat);
            }
        }
    }
}