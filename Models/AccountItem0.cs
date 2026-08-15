using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Account_Item0")]
public partial class AccountItem0
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDAccount")]
    public int Idaccount { get; set; }

    [Column("IDItem0")]
    public int Iditem0 { get; set; }

    public int Category { get; set; }

    [InverseProperty("IdaccountItem0Navigation")]
    public virtual ICollection<AccountItem0Attribute> AccountItem0Attributes { get; set; } = new List<AccountItem0Attribute>();

    [ForeignKey("Idaccount")]
    [InverseProperty("AccountItem0s")]
    public virtual Account IdaccountNavigation { get; set; } = null!;

    [ForeignKey("Iditem0")]
    [InverseProperty("AccountItem0s")]
    public virtual Item0 Iditem0Navigation { get; set; } = null!;
}
