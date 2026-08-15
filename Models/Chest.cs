using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Chest")]
[Index("Idaccount", Name = "UQ__Chest__1D323F913B8D19F1", IsUnique = true)]
public partial class Chest
{
    [Key]
    [Column("IDChest")]
    public int Idchest { get; set; }

    [Column("IDAccount")]
    public int? Idaccount { get; set; }

    [InverseProperty("IdchestNavigation")]
    public virtual ICollection<ChestItemX> ChestItemXes { get; set; } = new List<ChestItemX>();

    [ForeignKey("Idaccount")]
    [InverseProperty("Chest")]
    public virtual Account? IdaccountNavigation { get; set; }
}
