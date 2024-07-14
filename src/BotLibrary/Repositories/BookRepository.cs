using BookCollectorWebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace BookCollector.Repositories
{
    public class BookRepository(AppDbContext context) : IBookRepository
    {
        private readonly AppDbContext _context = context;

        public async Task<List<Book>> GetAllAsync()
        {
            return await _context.Books.ToListAsync();
        }
        public async Task<Book?> GetByIdAsync(Guid id)
        {
            var book = await _context.Books.FirstOrDefaultAsync(x => x.Id == id);
            return book;
        }
        public Task<List<Book>> GetByTitleAsync(string title)
        {
            var books = _context.Books.Where(x => x.Title.Contains(title)).ToList();
            return Task.FromResult(books);
        }

        public async Task AddAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Book book)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }
    }
}
