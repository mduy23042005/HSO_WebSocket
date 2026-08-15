using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Account_Item2")]
public partial class AccountItem2
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDAccount")]
    public int Idaccount { get; set; }

    [Column("IDItem2")]
    public int Iditem2 { get; set; }

    public int Level { get; set; }

    public int Quantity { get; set; }

    [ForeignKey("Idaccount")]
    [InverseProperty("AccountItem2s")]
    public virtual Account IdaccountNavigation { get; set; } = null!;

    [ForeignKey("Iditem2")]
    [InverseProperty("AccountItem2s")]
    public virtual Item2 Iditem2Navigation { get; set; } = null!;
}
