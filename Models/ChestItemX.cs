using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Chest_ItemX")]
public partial class ChestItemX
{
    [Key]
    [Column("IDChestItemX")]
    public int IdchestItemX { get; set; }

    [Column("IDChest")]
    public int? Idchest { get; set; }

    public int? TypeItem { get; set; }

    [Column("IDItemX")]
    public int? IditemX { get; set; }

    public int? Quantity { get; set; }

    [ForeignKey("Idchest")]
    [InverseProperty("ChestItemXes")]
    public virtual Chest? IdchestNavigation { get; set; }
}
