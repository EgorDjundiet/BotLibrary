using BookCollectorWebAPI.Models;
using System.ComponentModel.DataAnnotations;

namespace BookCollectorWebAPI
{
    public class BookDTOWrapper
    {
        public BookDTO BookDTO { get; set; }
        public BookDTOValidator BookDTOValidator { get; set; }
        public int CountValidatedProperties { get; set; }
        public List<string> PropertyNames { get; private set; } =
        [
            "Title", "Author", "Description", "Amazon Rating", "Book Outlet Rating", "Image", "Book Document"
        ];

        public BookDTOWrapper(BookDTO bookDTO, BookDTOValidator bookDTOValidator)
        {
            BookDTO = bookDTO;
            BookDTOValidator = bookDTOValidator;
        }
        public async Task HandleProperty(object? prop)
        {
            InitializeBookDTOPropertyAsync(prop);
            var result = await BookDTOValidator.PropertyValidators[CountValidatedProperties](BookDTO);
            if (!string.IsNullOrEmpty(result))
            {
                InitializeBookDTOPropertyAsync(null);
                throw new ValidationException(result);
            }
            CountValidatedProperties += 1;
            
            
        }
        private void InitializeBookDTOPropertyAsync(object? prop)
        {
            try
            {
                switch (CountValidatedProperties)
                {
                    case 0:
                        BookDTO.Title = prop as string ?? null;
                        break;
                    case 1:
                        BookDTO.Author = prop as string ?? null;
                        break;
                    case 2:
                        BookDTO.Description = prop as string ?? null;
                        break;
                    case 3:
                        BookDTO.AmazonRating = prop as string ?? null;
                        break;
                    case 4:
                        BookDTO.BookOutletRating = prop as string ?? null;
                        break;
                    case 5:
                        BookDTO.ImageInfo = prop as TelegramFileInfo ?? null;
                        break;
                    case 6:
                        BookDTO.BookDocumentInfo = prop as TelegramFileInfo ?? null;
                        break;
                }
            }
            catch(InvalidCastException)
            {
                throw new InvalidCastException($"{PropertyNames[CountValidatedProperties]} doesn't have correct format. Try again.");
            }
            
        }
    }
}
