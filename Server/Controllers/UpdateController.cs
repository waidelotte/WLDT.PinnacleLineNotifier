using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using WDLT.Frameworks.Telegram;

namespace Server.Controllers
{
    // TODO Unique URL
    [Route("")]
    public class UpdateController : Controller
    {
        private readonly TelegramFramework _framework;

        public UpdateController(TelegramFramework framework)
        {
            _framework = framework;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            if (update != null)
            {
                try
                {
                    await _framework.EchoAsync(update);
                }
                catch (Exception e)
                {
                    return Ok();
                }
            }

            return Ok();
        }
    }
}