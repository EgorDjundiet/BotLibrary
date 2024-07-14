using BookCollector;
using BookCollector.Repositories;
using BookCollector.Services;
using BookCollectorWebAPI.Middlewares;
using BookCollectorWebAPI.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Telegram.Bot;

namespace BookCollectorWebAPI
{
    public class Program
    {
        static BotLauncher? botLauncher = null;
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<ITelegramBotClient,TelegramBotClient>
                (provider => new TelegramBotClient(builder.Configuration.GetSection("TelegramBot:Token").Value!));
            builder.Services.AddScoped<FileHandler>();
            builder.Services.AddAutoMapper(typeof(MappingProfile));
            builder.Services.AddScoped<BookDTOValidator>();
            builder.Services.AddScoped<IBookService, BookService>();
            builder.Services.AddScoped<IBookRepository, BookRepository>();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddSingleton<ITelegramBotClient,TelegramBotClient>(p =>
            {
                var token = p.GetRequiredService<IConfiguration>().GetSection("TelegramBot:Token").Value;
                return new TelegramBotClient(token!);
            });
            var app = builder.Build();
            botLauncher = new BotLauncher(app.Services.GetRequiredService<ITelegramBotClient>(), app.Configuration);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.UseMiddleware<ExceptionHandlerMiddleware>();
            
            app.Run();
            
        }
    }
}
