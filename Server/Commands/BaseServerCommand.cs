using System;
using Database;
using Microsoft.Extensions.DependencyInjection;
using WDLT.Frameworks.Telegram.Models;

namespace Server.Commands
{
    public abstract class BaseServerCommand : BaseCommand
    {
        private readonly IServiceProvider _serviceProvider;

        protected BaseServerCommand(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected AppDatabase CreateScope()
        {
            return _serviceProvider.CreateScope().ServiceProvider.GetService<AppDatabase>();
        }
    }
}