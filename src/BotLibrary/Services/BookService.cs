using AutoMapper;
using BookCollector.Repositories;
using BookCollectorWebAPI;
using BookCollectorWebAPI.Models;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace BookCollector.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _repository;
        private readonly IMapper _mapper;
        private readonly ITelegramBotClient _bot;
        private readonly BookDTOValidator _bookDTOValidator;
        private readonly FileHandler _fileHandler;
        //1 User - 1 BookDto
        private readonly static Dictionary<long, BookDTOWrapper> BookDTOWrappers = [];

        public BookService(IBookRepository repository, IMapper mapper, ITelegramBotClient bot, BookDTOValidator bookDTOValidator, FileHandler fileHandler)
        {
            _repository = repository;
            _mapper = mapper;
            _bot = bot;
            _bookDTOValidator = bookDTOValidator;
            _fileHandler = fileHandler;
        }
        public async Task<List<BookDTO>> GetAllAsync()
        {
            var books = await _repository.GetAllAsync();
            var returnedBooks = books.Select(x => _mapper.Map<BookDTO>(x)).ToList();
            return returnedBooks;
        }
        public async Task<BookDTO?> GetByIdAsync(Guid id)
        {
            return _mapper.Map<BookDTO>((await _repository.GetByIdAsync(id)));
        }
        public async Task<List<BookDTO>> GetByTitleAsync(string title)
        {
            var books = await _repository.GetByTitleAsync(title);
            var returnedBooks = books.Select(x => _mapper.Map<BookDTO>(x)).ToList();
            return returnedBooks;
        }

        public async Task AddAsync(BookDTO book)
        {
            var bookEntity = _mapper.Map<Book>(book);
            await _repository.AddAsync(bookEntity);
        }

        public async Task UpdateAsync(Guid id,BookDTO book)
        {
            var bookEntity = _mapper.Map<Book>(book);
            bookEntity.Id = id;
            await _repository.UpdateAsync(bookEntity);
        }

        public async Task DeleteAsync(Guid id)
        {
            var book = await _repository.GetByIdAsync(id);
            await _repository.DeleteAsync(book);
        }

        public async Task HandleMessageAsync(Update update)
        {
            var message = update.Message;

            if (message == null)
                return;

            var chatId = message.Chat.Id;

            if (BookDTOWrappers.TryGetValue(chatId, out _))
            {
                if(message.Text != null)
                {
                    if (message.Text.StartsWith("/") && !message.Text.StartsWith("/skip") && !message.Text.StartsWith("/cancel"))
                    {
                        await _bot.SendTextMessageAsync(chatId, "You can't send commands during sending a book, besides /skip and /cancel");
                        return;
                    }
                    else if (message.Text.StartsWith("/skip"))
                    {
                        message.Text = null;
                        message.Photo = null;
                        message.Document = null;
                    }
                    else if (message.Text.StartsWith("/cancel"))
                    {
                        BookDTOWrappers.Remove(chatId);
                        await _bot.SendTextMessageAsync(chatId, "Sending the book is cancelled");
                        return;
                    }
                }
                
               
                var bookDTOWrapper = BookDTOWrappers[chatId];
                if (bookDTOWrapper.CountValidatedProperties < 5)
                {
                    await bookDTOWrapper.HandleProperty(update.Message!.Text!);
                }
                else if(bookDTOWrapper.CountValidatedProperties == 5)
                {
                    var file = await _fileHandler.DownloadFileAsBytesAsync(message.Photo?[^1].FileId);
                    await bookDTOWrapper.HandleProperty(file);
                }
                else
                {
                    var file = await _fileHandler.DownloadFileAsBytesAsync(message.Document?.FileId);
                    await bookDTOWrapper.HandleProperty(file);
                }


                if (bookDTOWrapper.CountValidatedProperties == 7)
                {
                    await AddAsync(bookDTOWrapper.BookDTO);
                    BookDTOWrappers.Remove(chatId);
                    await _bot.SendTextMessageAsync(chatId, "Your book is accepted.");
                    return;
                }
                await _bot.SendTextMessageAsync(chatId, $"Send {bookDTOWrapper.PropertyNames[bookDTOWrapper.CountValidatedProperties]} of your book");
            }

            else
            {
                if (message.Text == null) return;
                if (message.Text.StartsWith("/start"))
                {
                    await _bot.SendTextMessageAsync(chatId, "Welcome to the book collector! Here you can download books of all genres, as well as send your books.");
                    await _bot.SendTextMessageAsync(chatId, "To find the book you need, enter its title.");
                    await _bot.SendTextMessageAsync(chatId, "If you want to send your book, then enter the command /send.");
                    await _bot.SendTextMessageAsync(chatId, "If you want to cancel sending your book, then enter the command /cancel.");
                    await _bot.SendTextMessageAsync(chatId, "If you want to skip your book parameters, then enter the command /skip.");
                    return;
                }
                if (message.Text.StartsWith("/send"))
                {
                    var bookDTOWrapper = new BookDTOWrapper(new BookDTO(), _bookDTOValidator);
                    if (!BookDTOWrappers.TryGetValue(chatId, out _)) BookDTOWrappers.Add(chatId, bookDTOWrapper);
                    await _bot.SendTextMessageAsync(chatId, $"Send {bookDTOWrapper.PropertyNames[bookDTOWrapper.CountValidatedProperties]} of your book");
                    return;
                }
                else
                {
                    var books = await GetByTitleAsync(message.Text);
                    if(books == null || books.Count == 0) 
                    {
                        await _bot.SendTextMessageAsync(chatId, "Nothing was found for your request.");
                        return;
                    }

                    await _bot.SendTextMessageAsync(chatId, "List of books:");
                    for(int i = 0; i < books.Count; i++)
                    {
                        await _bot.SendTextMessageAsync(chatId, $"Book №{i+1}");
                        var book = books[i];
                        await _bot.SendTextMessageAsync(chatId, $"Title: {book.Title}");
                        await _bot.SendTextMessageAsync(chatId, $"Author: {book.Author}");
                        if (book.Description != null) await _bot.SendTextMessageAsync(chatId, $"Description: {book.Description}");
                        if (book.AmazonRating != null) await _bot.SendTextMessageAsync(chatId, $"Amazon rating: {book.AmazonRating}");
                        if (book.BookOutletRating != null) await _bot.SendTextMessageAsync(chatId, $"Book Outlet rating: {book.BookOutletRating}");
                        if (book.ImageInfo != null)
                        {
                            using (var stream = new MemoryStream(book.ImageInfo.Bytes))
                            {
                                InputFileStream inputFile = new InputFileStream(stream, book.ImageInfo.Name);
                                await _bot.SendPhotoAsync(chatId, inputFile);
                            }
                        }
                        
                        using (var stream = new MemoryStream(book.BookDocumentInfo!.Bytes))
                        {
                            InputFileStream inputFile = new InputFileStream(stream, book.BookDocumentInfo.Name);
                            await _bot.SendDocumentAsync(chatId, inputFile);
                        }
                    }
                }
            }
            
        }
    }
}
