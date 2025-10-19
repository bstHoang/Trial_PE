using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Server
{
    public class CreateOrderRequest
    {
        [JsonPropertyName("items")]
        public List<OrderItem> Items { get; set; }
    }
}