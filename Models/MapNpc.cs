using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Map_NPC")]
public partial class MapNpc
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDMap")]
    public int Idmap { get; set; }

    [Column("IDNPC")]
    public int Idnpc { get; set; }

    public int? PosX { get; set; }

    public int? PosY { get; set; }

    [ForeignKey("Idmap")]
    [InverseProperty("MapNpcs")]
    public virtual Map IdmapNavigation { get; set; } = null!;

    [ForeignKey("Idnpc")]
    [InverseProperty("MapNpcs")]
    public virtual Npc IdnpcNavigation { get; set; } = null!;
}
