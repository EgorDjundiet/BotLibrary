namespace BookCollectorWebAPI.Models
{
    public class Book
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal? AmazonRating { get; set; }
        public decimal? BookOutletRating { get; set; }
        public TelegramFileInfo? ImageInfo { get; set; }
        public TelegramFileInfo BookDocumentInfo { get; set; } = new([],"");
    }
}
