using BookCollectorWebAPI.Models;
using Telegram.Bot.Types;

namespace BookCollector.Services
{
    public interface IBookService
    {
        public Task HandleMessageAsync(Update update);
        public Task<List<BookDTO>> GetAllAsync();
        public Task<BookDTO?> GetByIdAsync(Guid id);
        public Task<List<BookDTO>> GetByTitleAsync(string title);
        public Task AddAsync(BookDTO book);
        public Task UpdateAsync(Guid id, BookDTO book);
        public Task DeleteAsync(Guid id);
    }
}
