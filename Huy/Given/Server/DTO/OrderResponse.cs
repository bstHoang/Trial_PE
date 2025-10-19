using System.Collections.Generic;

namespace Server
{
    public class OrderResponse
    {
        public int OrderId { get; set; }
        public string OrderDate { get; set; } // Format as "YYYY-MM-DD"
        public decimal TotalPrice { get; set; }
        public List<OrderDetailResponse> OrderDetails { get; set; }
    }
}