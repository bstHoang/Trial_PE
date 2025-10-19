namespace TCP_Client.Models
{
    public class Movie
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public int ReleaseYear { get; set; }
        public string Genre { get; set; }
        public int AvailableCopies { get; set; }
        public Director Director { get; set; }
    }
}
