using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Map_Mob")]
public partial class MapMob
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDMap")]
    public int Idmap { get; set; }

    [Column("IDMob")]
    public int Idmob { get; set; }

    public int? PosX { get; set; }

    public int? PosY { get; set; }

    [ForeignKey("Idmap")]
    [InverseProperty("MapMobs")]
    public virtual Map IdmapNavigation { get; set; } = null!;

    [ForeignKey("Idmob")]
    [InverseProperty("MapMobs")]
    public virtual Mob IdmobNavigation { get; set; } = null!;
}
