using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Item0_Attribute")]
public partial class Item0Attribute
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDItem0")]
    public int Iditem0 { get; set; }

    [Column("IDAttribute")]
    public int Idattribute { get; set; }

    public int Value { get; set; }

    public int Category { get; set; }

    [ForeignKey("Idattribute")]
    [InverseProperty("Item0Attributes")]
    public virtual Attribute IdattributeNavigation { get; set; } = null!;

    [ForeignKey("Iditem0")]
    [InverseProperty("Item0Attributes")]
    public virtual Item0 Iditem0Navigation { get; set; } = null!;
}
