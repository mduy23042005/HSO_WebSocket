using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Item3")]
public partial class Item3
{
    [Key]
    [Column("IDItem3")]
    public int Iditem3 { get; set; }

    [StringLength(255)]
    public string NameItem3 { get; set; } = null!;

    public string? Details { get; set; }

    [InverseProperty("Iditem3Navigation")]
    public virtual ICollection<AccountItem3> AccountItem3s { get; set; } = new List<AccountItem3>();
}
