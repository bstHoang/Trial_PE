using System;
using System.Collections.Generic;

namespace Project11.Models;

public partial class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
