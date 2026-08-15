using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Account_Item4")]
public partial class AccountItem4
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDAccount")]
    public int Idaccount { get; set; }

    [Column("IDItem4")]
    public int Iditem4 { get; set; }

    public int Level { get; set; }

    [ForeignKey("Idaccount")]
    [InverseProperty("AccountItem4s")]
    public virtual Account IdaccountNavigation { get; set; } = null!;

    [ForeignKey("Iditem4")]
    [InverseProperty("AccountItem4s")]
    public virtual Item4 Iditem4Navigation { get; set; } = null!;
}
