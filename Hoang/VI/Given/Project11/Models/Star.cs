using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Project11.Models;

public partial class Star
{
    [Key]
    [Column("StarID")]
    public int StarId { get; set; }

    [StringLength(50)]
    public string? FirstName { get; set; }

    [StringLength(50)]
    public string? LastName { get; set; }

    public DateOnly? BirthDate { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [InverseProperty("Star")]
    public virtual ICollection<MovieStar> MovieStars { get; set; } = new List<MovieStar>();
}
