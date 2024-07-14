using BookCollectorWebAPI.Models;

namespace BookCollector.Repositories
{
    public interface IBookRepository
    {
        public Task<List<Book>> GetAllAsync();
        public Task<Book?> GetByIdAsync(Guid id);
        public Task<List<Book>> GetByTitleAsync(string title);
        public Task AddAsync(Book book);
        public Task UpdateAsync(Book book);
        public Task DeleteAsync(Book book);
    }
}
