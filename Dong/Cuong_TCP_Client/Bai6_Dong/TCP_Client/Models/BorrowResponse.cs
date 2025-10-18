namespace TCP_Client.Models
{
    public class BorrowResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public Movie Movie { get; set; }
    }
}
