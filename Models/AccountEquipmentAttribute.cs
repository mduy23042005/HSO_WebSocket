using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Account_Equipment_Attribute")]
public partial class AccountEquipmentAttribute
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDAccountEquipment")]
    public int IdaccountEquipment { get; set; }

    [Column("IDAttribute")]
    public int Idattribute { get; set; }

    public int? Value { get; set; }

    [ForeignKey("IdaccountEquipment")]
    [InverseProperty("AccountEquipmentAttributes")]
    public virtual AccountEquipment IdaccountEquipmentNavigation { get; set; } = null!;

    [ForeignKey("Idattribute")]
    [InverseProperty("AccountEquipmentAttributes")]
    public virtual Attribute IdattributeNavigation { get; set; } = null!;
}
