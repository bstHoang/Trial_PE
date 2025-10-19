using System.Net;

namespace Client
{
    public class BookDto
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public int PublicationYear { get; set; }
        public string Genres { get; set; }
        public List<AuthorDto> Authors { get; set; }
    }

    public class AuthorDto
    {
        public string Name { get; set; }
        public int BirthYear { get; set; }
    }
}
