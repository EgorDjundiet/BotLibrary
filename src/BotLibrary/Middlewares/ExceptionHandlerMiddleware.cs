using System.ComponentModel.DataAnnotations;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BookCollectorWebAPI.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ITelegramBotClient _bot;

        public ExceptionHandlerMiddleware(RequestDelegate next, ITelegramBotClient bot)
        {
            _next = next;
            _bot = bot;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is ValidationException)  
            {
                if (context.Items.ContainsKey("Update"))
                {
                    var update = context.Items["Update"] as Update;
                    await _bot.SendTextMessageAsync(update!.Message!.Chat.Id, ex.Message);
                }

            }
            catch 
            {
                if (context.Items.ContainsKey("Update"))
                {
                    var update = context.Items["Update"] as Update;
                    await _bot.SendTextMessageAsync(update!.Message!.Chat.Id, "Something went wrong.");
                }
            }
        }
    }
}
