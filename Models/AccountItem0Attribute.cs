using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Account_Item0_Attribute")]
public partial class AccountItem0Attribute
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDAccountItem0")]
    public int IdaccountItem0 { get; set; }

    [Column("IDAttribute")]
    public int Idattribute { get; set; }

    public int Value { get; set; }

    [ForeignKey("IdaccountItem0")]
    [InverseProperty("AccountItem0Attributes")]
    public virtual AccountItem0 IdaccountItem0Navigation { get; set; } = null!;

    [ForeignKey("Idattribute")]
    [InverseProperty("AccountItem0Attributes")]
    public virtual Attribute IdattributeNavigation { get; set; } = null!;
}
