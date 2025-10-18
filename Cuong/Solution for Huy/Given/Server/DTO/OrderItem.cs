using System.Text.Json.Serialization;

namespace Server
{
    public class OrderItem
    {
        [JsonPropertyName("ProductId")]
        public int ProductId { get; set; }

        [JsonPropertyName("Quantity")]
        public int Quantity { get; set; }
    }
}