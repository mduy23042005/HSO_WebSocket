using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("School")]
[Index("NameSchool", Name = "UQ__School__98FBD932C7BCF4E5", IsUnique = true)]
public partial class School
{
    [Key]
    [Column("IDSchool")]
    public int Idschool { get; set; }

    [StringLength(255)]
    public string? NameSchool { get; set; }

    [InverseProperty("IdschoolNavigation")]
    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    [InverseProperty("IdschoolNavigation")]
    public virtual ICollection<Item0> Item0s { get; set; } = new List<Item0>();

    [InverseProperty("IdschoolNavigation")]
    public virtual ICollection<Skill> Skills { get; set; } = new List<Skill>();
}
