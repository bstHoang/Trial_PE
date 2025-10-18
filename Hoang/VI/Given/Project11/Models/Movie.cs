using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project11.Models;

public partial class Movie
{
    [Key]
    [Column("MovieID")]
    public int MovieId { get; set; }

    [StringLength(100)]
    public string Title { get; set; } = null!;

    public int? ReleaseYear { get; set; }

    [StringLength(50)]
    public string? Genre { get; set; }

    public int? AvailableCopies { get; set; }

    [Column("DirectorID")]
    public int? DirectorId { get; set; }

    [ForeignKey("DirectorId")]
    [InverseProperty("Movies")]
    public virtual Director? Director { get; set; }

    [InverseProperty("Movie")]
    public virtual ICollection<MovieStar> MovieStars { get; set; } = new List<MovieStar>();

    [InverseProperty("Movie")]
    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
}
