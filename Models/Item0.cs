using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Item0")]
public partial class Item0
{
    [Key]
    [Column("IDItem0")]
    public int Iditem0 { get; set; }

    [StringLength(255)]
    public string NameItem0 { get; set; } = null!;

    [Column("IDSchool")]
    public int? Idschool { get; set; }

    public int? Level { get; set; }

    public string? TypeItem0 { get; set; }

    [InverseProperty("Iditem0Navigation")]
    public virtual ICollection<AccountItem0> AccountItem0s { get; set; } = new List<AccountItem0>();

    [ForeignKey("Idschool")]
    [InverseProperty("Item0s")]
    public virtual School? IdschoolNavigation { get; set; }

    [InverseProperty("Iditem0Navigation")]
    public virtual ICollection<Item0Attribute> Item0Attributes { get; set; } = new List<Item0Attribute>();
}
