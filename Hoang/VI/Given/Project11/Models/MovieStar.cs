using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project11.Models;

[PrimaryKey("MovieId", "StarId")]
public partial class MovieStar
{
    [Key]
    [Column("MovieID")]
    public int MovieId { get; set; }

    [Key]
    [Column("StarID")]
    public int StarId { get; set; }

    [StringLength(100)]
    public string? RoleName { get; set; }

    [ForeignKey("MovieId")]
    [InverseProperty("MovieStars")]
    public virtual Movie Movie { get; set; } = null!;

    [ForeignKey("StarId")]
    [InverseProperty("MovieStars")]
    public virtual Star Star { get; set; } = null!;
}
