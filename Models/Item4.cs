using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Item4")]
public partial class Item4
{
    [Key]
    [Column("IDItem4")]
    public int Iditem4 { get; set; }

    [StringLength(255)]
    public string? NameItem4 { get; set; }

    public string? Details { get; set; }

    [InverseProperty("Iditem4Navigation")]
    public virtual ICollection<AccountItem4> AccountItem4s { get; set; } = new List<AccountItem4>();
}
