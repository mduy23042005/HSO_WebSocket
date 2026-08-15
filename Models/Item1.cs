using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Item1")]
public partial class Item1
{
    [Key]
    [Column("IDItem1")]
    public int Iditem1 { get; set; }

    public string? NameItem1 { get; set; }

    public string? TypeItem1 { get; set; }

    [InverseProperty("Iditem1Navigation")]
    public virtual ICollection<AccountItem1> AccountItem1s { get; set; } = new List<AccountItem1>();

    [InverseProperty("Iditem1Navigation")]
    public virtual ICollection<Item1Attribute> Item1Attributes { get; set; } = new List<Item1Attribute>();
}
