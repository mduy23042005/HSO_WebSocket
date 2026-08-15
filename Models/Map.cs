using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Map")]
public partial class Map
{
    [Key]
    [Column("IDMap")]
    public int Idmap { get; set; }

    [StringLength(255)]
    public string NameMap { get; set; } = null!;

    [InverseProperty("IdmapNavigation")]
    public virtual ICollection<MapMob> MapMobs { get; set; } = new List<MapMob>();

    [InverseProperty("IdmapNavigation")]
    public virtual ICollection<MapNpc> MapNpcs { get; set; } = new List<MapNpc>();
}
