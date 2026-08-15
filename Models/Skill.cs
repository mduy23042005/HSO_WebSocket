using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Skill")]
public partial class Skill
{
    [Key]
    [Column("IDSkill")]
    public int Idskill { get; set; }

    [Column("IDSchool")]
    public int? Idschool { get; set; }

    [StringLength(255)]
    public string? NameSkill { get; set; }

    public int? NumAttackTargets { get; set; }

    public int? LevelRequired { get; set; }

    public int? BuffEffect { get; set; }

    public string? Details { get; set; }

    public int? Damage { get; set; }

    public int? ManaCost { get; set; }

    public int? Cooldown { get; set; }

    [ForeignKey("Idschool")]
    [InverseProperty("Skills")]
    public virtual School? IdschoolNavigation { get; set; }
}
