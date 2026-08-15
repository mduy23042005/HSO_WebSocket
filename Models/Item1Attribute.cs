using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Item1_Attribute")]
public partial class Item1Attribute
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDItem1")]
    public int Iditem1 { get; set; }

    [Column("IDAttribute")]
    public int Idattribute { get; set; }

    public int Value { get; set; }

    public int Category { get; set; }

    [ForeignKey("Idattribute")]
    [InverseProperty("Item1Attributes")]
    public virtual Attribute IdattributeNavigation { get; set; } = null!;

    [ForeignKey("Iditem1")]
    [InverseProperty("Item1Attributes")]
    public virtual Item1 Iditem1Navigation { get; set; } = null!;
}
