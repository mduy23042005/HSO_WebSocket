using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("Account")]
public partial class Account
{
    [Key]
    [Column("IDAccount")]
    public int Idaccount { get; set; }

    [StringLength(255)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string Password { get; set; } = null!;

    [StringLength(255)]
    public string? NameChar { get; set; }

    [Column("IDSchool")]
    public int? Idschool { get; set; }

    public int? Level { get; set; }

    public int? SkillPoints { get; set; }

    public int? StatPoints { get; set; }

    public int? Exp { get; set; }

    public int? Hair { get; set; }

    public int? Gold { get; set; }

    public int? Gem { get; set; }

    public int? Point0 { get; set; }

    public int? Point1 { get; set; }

    public int? Point2 { get; set; }

    public int? Point3 { get; set; }

    public int? PointArena { get; set; }

    public int? PointActive { get; set; }

    public int? Skill0 { get; set; }

    public int? Skill1 { get; set; }

    public int? Skill2 { get; set; }

    public int? Skill3 { get; set; }

    public int? Skill4 { get; set; }

    public int? Skill5 { get; set; }

    public int? Skill6 { get; set; }

    public int? Skill7 { get; set; }

    public int? Skill8 { get; set; }

    public int? Skill9 { get; set; }

    public int? Skill10 { get; set; }

    public int? Skill11 { get; set; }

    public int? Skill12 { get; set; }

    public int? Skill13 { get; set; }

    public int? Skill14 { get; set; }

    public int? Skill15 { get; set; }

    public int? Skill16 { get; set; }

    public int? Skill17 { get; set; }

    public int? Skill18 { get; set; }

    public int? Skill19 { get; set; }

    public int? Skill20 { get; set; }

    [StringLength(255)]
    public string? Clan { get; set; }

    public int BlessingPoints { get; set; }

    [InverseProperty("IdaccountNavigation")]
    public virtual ICollection<AccountEquipment> AccountEquipments { get; set; } = new List<AccountEquipment>();

    [InverseProperty("IdaccountNavigation")]
    public virtual ICollection<AccountItem0> AccountItem0s { get; set; } = new List<AccountItem0>();

    [InverseProperty("IdaccountNavigation")]
    public virtual ICollection<AccountItem1> AccountItem1s { get; set; } = new List<AccountItem1>();

    [InverseProperty("IdaccountNavigation")]
    public virtual ICollection<AccountItem2> AccountItem2s { get; set; } = new List<AccountItem2>();

    [InverseProperty("IdaccountNavigation")]
    public virtual ICollection<AccountItem3> AccountItem3s { get; set; } = new List<AccountItem3>();

    [InverseProperty("IdaccountNavigation")]
    public virtual ICollection<AccountItem4> AccountItem4s { get; set; } = new List<AccountItem4>();

    [InverseProperty("IdaccountNavigation")]
    public virtual Chest? Chest { get; set; }

    [ForeignKey("Idschool")]
    [InverseProperty("Accounts")]
    public virtual School? IdschoolNavigation { get; set; }
}
