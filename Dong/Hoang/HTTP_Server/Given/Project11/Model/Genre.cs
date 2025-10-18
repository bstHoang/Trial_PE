using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Project11.Model;

public partial class Genre
{
    public int GenreId { get; set; }

    public string GenreName { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
