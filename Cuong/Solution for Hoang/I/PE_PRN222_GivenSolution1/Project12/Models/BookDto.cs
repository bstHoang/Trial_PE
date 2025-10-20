namespace Project12.Models
{
    public class BookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? PublicationYear { get; set; }
        public string GenreName { get; set; } = string.Empty;
    }
}
