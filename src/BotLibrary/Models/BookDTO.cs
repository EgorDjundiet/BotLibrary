using BookCollector;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BookCollectorWebAPI.Models
{
    public class BookDTO
    {
        public string? Title { get; set; } 
        public string? Author { get; set; }
        public string? Description { get; set; }
        public string? AmazonRating { get; set; }
        public string? BookOutletRating { get; set; }
        public TelegramFileInfo? ImageInfo { get; set; }
        public TelegramFileInfo? BookDocumentInfo { get; set; }
    }

    public class BookDTOValidator
    {
        private readonly FileHandler _fileHandler;
        public List<Func<BookDTO, Task<string>>> PropertyValidators { get; private set; }
        
        public BookDTOValidator(FileHandler fileHandler)
        {
            _fileHandler = fileHandler;

            PropertyValidators =
            [
                book => Task.FromResult(ValidateTitle(book.Title)),
                book => Task.FromResult(ValidateAuthor(book.Author)),
                book => Task.FromResult(ValidateDescription(book.Description)),
                book => Task.FromResult(ValidateAmazonRating(book.AmazonRating)),
                book => Task.FromResult(ValidateBookOutletRating(book.BookOutletRating)),
                async (book) => await ValidateImageAsync(book.ImageInfo),
                async (book) => await ValidateBookDocumentAsync(book.BookDocumentInfo)
            ];

            
        }

        // Instance methods for validation
        private string ValidateTitle(string? title)
        {
            return string.IsNullOrEmpty(title?.Trim()) ? "Title is required." : "";
        }

        private string ValidateAuthor(string? author)
        {
            return string.IsNullOrEmpty(author?.Trim()) ? "Author is required." : "";
        }

        private string ValidateDescription(string? description)
        {
            return "";
        }

        private string ValidateAmazonRating(string? amazonRating)
        {
            return amazonRating == null || IsAValidRating(amazonRating) ? "" : "Amazon rating must be a number between 0 and 5.";
        }

        private string ValidateBookOutletRating(string? bookOutletRating)
        {
            return bookOutletRating == null || IsAValidRating(bookOutletRating) ? "" : "Book Outlet rating must be a number between 0 and 5.";
        }

        private async Task<string> ValidateImageAsync(TelegramFileInfo? imageInfo)
        {
            string messages = "";
            if (imageInfo != null)
            {
                if (!_fileHandler.IsImage(imageInfo))
                    messages += "File doesn't have supported extension for this image.";
                if (!await _fileHandler.IsSecureFileAsync(imageInfo))
                    messages += "File is not secure.";
            }
            return messages;
        }

        private async Task<string> ValidateBookDocumentAsync(TelegramFileInfo? bookDocumentInfo)
        {
            if (bookDocumentInfo == null)
                return "Book document is required.";
            string messages = "";
            if (!_fileHandler.IsBookDocument(bookDocumentInfo))
                messages += "File doesn't have supported extension for this book document.";
            if (!await _fileHandler.IsSecureFileAsync(bookDocumentInfo))
                messages += "File is not secure.";

            return messages;
        }

        // Utility method to validate rating
        private static bool IsAValidRating(string rating)
        {
            if (double.TryParse(rating, out double parsedRating))
            {
                return parsedRating >= 0 && parsedRating <= 5;
            }
            return false;
        }
    }

}
