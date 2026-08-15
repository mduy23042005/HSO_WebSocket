using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Mob")]
public partial class Mob
{
    [Key]
    [Column("IDMob")]
    public int Idmob { get; set; }

    [StringLength(255)]
    public string NameMob { get; set; } = null!;

    public bool Boss { get; set; }

    public int? Level { get; set; }

    [Column("HP")]
    public int? Hp { get; set; }

    [InverseProperty("IdmobNavigation")]
    public virtual ICollection<MapMob> MapMobs { get; set; } = new List<MapMob>();
}
