using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project11.Models;

public partial class Review
{
    [Key]
    [Column("ReviewID")]
    public int ReviewId { get; set; }

    [Column("MovieID")]
    public int? MovieId { get; set; }

    [StringLength(100)]
    public string? ReviewerName { get; set; }

    [Column(TypeName = "decimal(2, 1)")]
    public decimal? Rating { get; set; }

    [StringLength(500)]
    public string? Comment { get; set; }

    public DateOnly? ReviewDate { get; set; }

    [ForeignKey("MovieId")]
    [InverseProperty("Reviews")]
    public virtual Movie? Movie { get; set; }
}
