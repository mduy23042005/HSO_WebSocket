using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Account_Item3")]
public partial class AccountItem3
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDAccount")]
    public int Idaccount { get; set; }

    [Column("IDItem3")]
    public int Iditem3 { get; set; }

    public int Level { get; set; }

    public int Quantity { get; set; }

    [ForeignKey("Idaccount")]
    [InverseProperty("AccountItem3s")]
    public virtual Account IdaccountNavigation { get; set; } = null!;

    [ForeignKey("Iditem3")]
    [InverseProperty("AccountItem3s")]
    public virtual Item3 Iditem3Navigation { get; set; } = null!;
}
