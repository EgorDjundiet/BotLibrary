using BookCollector.Services;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace BookCollectorWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IBookService _bookService;
        public MessageController(IBookService service)
        {
            _bookService = service;
        }

        [HttpPost]
        public async Task Post([FromBody]Update update)
        {
            HttpContext.Items["Update"] = update;
            await _bookService.HandleMessageAsync(update);
        }
    }
}
