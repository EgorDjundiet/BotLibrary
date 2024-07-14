using Newtonsoft.Json;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace BookCollectorWebAPI
{
    //Do not register in DI container
    //Just create instance in Program.cs
    public class BotLauncher
    {
        private readonly ITelegramBotClient _bot;
        private readonly IConfiguration _config;
        public BotLauncher(ITelegramBotClient bot, IConfiguration config)
        {
            _bot = bot;
            _config = config;
            _bot.StartReceiving(HandleUpdate, HandleError);
        }
        private async Task HandleUpdate(ITelegramBotClient client, Update update, CancellationToken token)
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var json = JsonConvert.SerializeObject(update,Formatting.Indented);
                    
                    var content = new StringContent(json, Encoding.UTF8,"application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(_config.GetSection("App:Link").Value,content);
                    response.EnsureSuccessStatusCode(); // Throw if not a success code.

                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                }
                catch (HttpRequestException e)
                {
                    Console.WriteLine("\nException Caught!");
                    Console.WriteLine("Message :{0} ", e.Message);
                }
            }
            
        }

        private async Task HandleError(ITelegramBotClient client, Exception exception, HandleErrorSource source, CancellationToken token)
        {
            await Console.Out.WriteLineAsync($"Error: {exception.Message}");
        }


    }
}
