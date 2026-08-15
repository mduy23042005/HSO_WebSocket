using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Attribute")]
public partial class Attribute
{
    [Key]
    [Column("IDAttribute")]
    public int Idattribute { get; set; }

    [StringLength(255)]
    public string NameAttribute { get; set; } = null!;

    [InverseProperty("IdattributeNavigation")]
    public virtual ICollection<AccountEquipmentAttribute> AccountEquipmentAttributes { get; set; } = new List<AccountEquipmentAttribute>();

    [InverseProperty("IdattributeNavigation")]
    public virtual ICollection<AccountItem0Attribute> AccountItem0Attributes { get; set; } = new List<AccountItem0Attribute>();

    [InverseProperty("IdattributeNavigation")]
    public virtual ICollection<Item0Attribute> Item0Attributes { get; set; } = new List<Item0Attribute>();

    [InverseProperty("IdattributeNavigation")]
    public virtual ICollection<Item1Attribute> Item1Attributes { get; set; } = new List<Item1Attribute>();
}
