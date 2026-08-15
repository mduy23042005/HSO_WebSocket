using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

[Table("ExpRequired")]
public partial class ExpRequired
{
    [Key]
    [Column("ID")]
    public int Id { get; set; }

    public int Level { get; set; }

    public int Required { get; set; }
}
