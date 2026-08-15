using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Account_Equipment")]
public partial class AccountEquipment
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    [Column("IDAccount")]
    public int Idaccount { get; set; }

    [Column("IDItem0_1")]
    public int Iditem01 { get; set; }

    [StringLength(50)]
    public string SlotName { get; set; } = null!;

    public int Category { get; set; }

    [InverseProperty("IdaccountEquipmentNavigation")]
    public virtual ICollection<AccountEquipmentAttribute> AccountEquipmentAttributes { get; set; } = new List<AccountEquipmentAttribute>();

    [ForeignKey("Idaccount")]
    [InverseProperty("AccountEquipments")]
    public virtual Account IdaccountNavigation { get; set; } = null!;
}
