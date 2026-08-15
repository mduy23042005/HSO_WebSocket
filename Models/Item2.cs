using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Item2")]
public partial class Item2
{
    [Key]
    [Column("IDItem2")]
    public int Iditem2 { get; set; }

    [StringLength(255)]
    public string NameItem2 { get; set; } = null!;

    [InverseProperty("Iditem2Navigation")]
    public virtual ICollection<AccountItem2> AccountItem2s { get; set; } = new List<AccountItem2>();
}
