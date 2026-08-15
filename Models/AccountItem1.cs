using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Account_Item1")]
public partial class AccountItem1
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDAccount")]
    public int Idaccount { get; set; }

    [Column("IDItem1")]
    public int Iditem1 { get; set; }

    public int Level { get; set; }

    [ForeignKey("Idaccount")]
    [InverseProperty("AccountItem1s")]
    public virtual Account IdaccountNavigation { get; set; } = null!;

    [ForeignKey("Iditem1")]
    [InverseProperty("AccountItem1s")]
    public virtual Item1 Iditem1Navigation { get; set; } = null!;
}
