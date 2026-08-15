using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("NPC")]
public partial class Npc
{
    [Key]
    [Column("IDNPC")]
    public int Idnpc { get; set; }

    [Column("NameNPC")]
    public string NameNpc { get; set; } = null!;

    [InverseProperty("IdnpcNavigation")]
    public virtual ICollection<MapNpc> MapNpcs { get; set; } = new List<MapNpc>();
}
