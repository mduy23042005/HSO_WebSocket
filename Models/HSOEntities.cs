using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HSO_Server.Models;

public partial class HSOEntities : DbContext
{
    public HSOEntities()
    {
    }

    public HSOEntities(DbContextOptions<HSOEntities> options)
        : base(options)
    {
    }

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<AccountEquipment> AccountEquipments { get; set; }

    public virtual DbSet<AccountEquipmentAttribute> AccountEquipmentAttributes { get; set; }

    public virtual DbSet<AccountItem0> AccountItem0s { get; set; }

    public virtual DbSet<AccountItem0Attribute> AccountItem0Attributes { get; set; }

    public virtual DbSet<AccountItem1> AccountItem1s { get; set; }

    public virtual DbSet<AccountItem2> AccountItem2s { get; set; }

    public virtual DbSet<AccountItem3> AccountItem3s { get; set; }

    public virtual DbSet<AccountItem4> AccountItem4s { get; set; }

    public virtual DbSet<Attribute> Attributes { get; set; }

    public virtual DbSet<Chest> Chests { get; set; }

    public virtual DbSet<ChestItemX> ChestItemXes { get; set; }

    public virtual DbSet<ExpRequired> ExpRequireds { get; set; }

    public virtual DbSet<Item0> Item0s { get; set; }

    public virtual DbSet<Item0Attribute> Item0Attributes { get; set; }

    public virtual DbSet<Item1> Item1s { get; set; }

    public virtual DbSet<Item1Attribute> Item1Attributes { get; set; }

    public virtual DbSet<Item2> Item2s { get; set; }

    public virtual DbSet<Item3> Item3s { get; set; }

    public virtual DbSet<Item4> Item4s { get; set; }

    public virtual DbSet<Map> Maps { get; set; }

    public virtual DbSet<MapMob> MapMobs { get; set; }

    public virtual DbSet<MapNpc> MapNpcs { get; set; }

    public virtual DbSet<Mob> Mobs { get; set; }

    public virtual DbSet<Npc> Npcs { get; set; }

    public virtual DbSet<School> Schools { get; set; }

    public virtual DbSet<Skill> Skills { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=LAPTOP-AC2MH2TQ\\THELMOD;Database=HSO;User Id=sa;Password=mduy23042005;TrustServerCertificate=True;MultipleActiveResultSets=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Idaccount).HasName("PK__Account__1D323F90280A0438");

            entity.Property(e => e.Exp).HasDefaultValue(0);
            entity.Property(e => e.Gem).HasDefaultValue(2000);
            entity.Property(e => e.Gold).HasDefaultValue(20000);
            entity.Property(e => e.Hair).HasDefaultValue(0);
            entity.Property(e => e.Level).HasDefaultValue(1);
            entity.Property(e => e.Point0).HasDefaultValue(5);
            entity.Property(e => e.Point1).HasDefaultValue(5);
            entity.Property(e => e.Point2).HasDefaultValue(5);
            entity.Property(e => e.Point3).HasDefaultValue(5);
            entity.Property(e => e.PointActive).HasDefaultValue(100000);
            entity.Property(e => e.PointArena).HasDefaultValue(0);
            entity.Property(e => e.Skill0).HasDefaultValue(1);
            entity.Property(e => e.Skill1).HasDefaultValue(0);
            entity.Property(e => e.Skill10).HasDefaultValue(0);
            entity.Property(e => e.Skill11).HasDefaultValue(0);
            entity.Property(e => e.Skill12).HasDefaultValue(0);
            entity.Property(e => e.Skill13).HasDefaultValue(0);
            entity.Property(e => e.Skill14).HasDefaultValue(0);
            entity.Property(e => e.Skill15).HasDefaultValue(0);
            entity.Property(e => e.Skill16).HasDefaultValue(0);
            entity.Property(e => e.Skill17).HasDefaultValue(0);
            entity.Property(e => e.Skill18).HasDefaultValue(0);
            entity.Property(e => e.Skill19).HasDefaultValue(0);
            entity.Property(e => e.Skill2).HasDefaultValue(0);
            entity.Property(e => e.Skill20).HasDefaultValue(0);
            entity.Property(e => e.Skill3).HasDefaultValue(0);
            entity.Property(e => e.Skill4).HasDefaultValue(0);
            entity.Property(e => e.Skill5).HasDefaultValue(0);
            entity.Property(e => e.Skill6).HasDefaultValue(0);
            entity.Property(e => e.Skill7).HasDefaultValue(0);
            entity.Property(e => e.Skill8).HasDefaultValue(0);
            entity.Property(e => e.Skill9).HasDefaultValue(0);
            entity.Property(e => e.SkillPoints).HasDefaultValue(0);
            entity.Property(e => e.StatPoints).HasDefaultValue(0);

            entity.HasOne(d => d.IdschoolNavigation).WithMany(p => p.Accounts).HasConstraintName("FK_Account_School");
        });

        modelBuilder.Entity<AccountEquipment>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account___3214EC277E4A0CDE");

            entity.Property(e => e.Category).HasDefaultValue(1);

            entity.HasOne(d => d.IdaccountNavigation).WithMany(p => p.AccountEquipments)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountEquipment_Account");
        });

        modelBuilder.Entity<AccountEquipmentAttribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account___3214EC2792AA80F4");

            entity.HasOne(d => d.IdaccountEquipmentNavigation).WithMany(p => p.AccountEquipmentAttributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountEquipmentAttribute_AccountEquipment");

            entity.HasOne(d => d.IdattributeNavigation).WithMany(p => p.AccountEquipmentAttributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountEquipmentAttribute_Attribute");
        });

        modelBuilder.Entity<AccountItem0>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account___3214EC276E9B21BE");

            entity.HasOne(d => d.IdaccountNavigation).WithMany(p => p.AccountItem0s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Item0_Account");

            entity.HasOne(d => d.Iditem0Navigation).WithMany(p => p.AccountItem0s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Item0_Item0");
        });

        modelBuilder.Entity<AccountItem0Attribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account___3214EC279645AD37");

            entity.HasOne(d => d.IdaccountItem0Navigation).WithMany(p => p.AccountItem0Attributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountItem0Attribute_AccountItem0");

            entity.HasOne(d => d.IdattributeNavigation).WithMany(p => p.AccountItem0Attributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_AccountItem0Attribute_Attribute");
        });

        modelBuilder.Entity<AccountItem1>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account___3214EC277F422C4A");

            entity.HasOne(d => d.IdaccountNavigation).WithMany(p => p.AccountItem1s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Item1_Account");

            entity.HasOne(d => d.Iditem1Navigation).WithMany(p => p.AccountItem1s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Item1_Item1");
        });

        modelBuilder.Entity<AccountItem2>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account___3214EC27DF5DDF34");

            entity.HasOne(d => d.IdaccountNavigation).WithMany(p => p.AccountItem2s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Item2_Account");

            entity.HasOne(d => d.Iditem2Navigation).WithMany(p => p.AccountItem2s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Item2_Item2");
        });

        modelBuilder.Entity<AccountItem3>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account___3214EC27EF07AD90");

            entity.HasOne(d => d.IdaccountNavigation).WithMany(p => p.AccountItem3s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Item3_Account");

            entity.HasOne(d => d.Iditem3Navigation).WithMany(p => p.AccountItem3s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Item3_Item3");
        });

        modelBuilder.Entity<AccountItem4>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account___3214EC278FD38A2A");

            entity.HasOne(d => d.IdaccountNavigation).WithMany(p => p.AccountItem4s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Account_I__IDAcc__2BC97F7C");

            entity.HasOne(d => d.Iditem4Navigation).WithMany(p => p.AccountItem4s)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Account_I__IDIte__2CBDA3B5");
        });

        modelBuilder.Entity<Attribute>(entity =>
        {
            entity.HasKey(e => e.Idattribute).HasName("PK__Attribut__3F710007BDDA1D14");
        });

        modelBuilder.Entity<Chest>(entity =>
        {
            entity.HasKey(e => e.Idchest).HasName("PK__Chest__50F23290CDE41837");

            entity.HasOne(d => d.IdaccountNavigation).WithOne(p => p.Chest).HasConstraintName("FK__Chest__IDAccount__1D7B6025");
        });

        modelBuilder.Entity<ChestItemX>(entity =>
        {
            entity.HasKey(e => e.IdchestItemX).HasName("PK__Chest_It__854FC8936AD5C18E");

            entity.Property(e => e.Quantity).HasDefaultValue(1);

            entity.HasOne(d => d.IdchestNavigation).WithMany(p => p.ChestItemXes).HasConstraintName("FK__Chest_Ite__IDChe__214BF109");
        });

        modelBuilder.Entity<ExpRequired>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ExpRequi__3214EC27519EF5E0");
        });

        modelBuilder.Entity<Item0>(entity =>
        {
            entity.HasKey(e => e.Iditem0).HasName("PK__Item0__D4E826C4EA045AA5");

            entity.Property(e => e.Idschool).HasDefaultValue(0);
            entity.Property(e => e.Level).HasDefaultValue(1);

            entity.HasOne(d => d.IdschoolNavigation).WithMany(p => p.Item0s).HasConstraintName("FK_Item0_School");
        });

        modelBuilder.Entity<Item0Attribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Item0_At__3214EC277FBD4119");

            entity.HasOne(d => d.IdattributeNavigation).WithMany(p => p.Item0Attributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item0_Attribute_Attribute");

            entity.HasOne(d => d.Iditem0Navigation).WithMany(p => p.Item0Attributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item0_Attribute_Item0");
        });

        modelBuilder.Entity<Item1>(entity =>
        {
            entity.HasKey(e => e.Iditem1).HasName("PK__Item1__D4E826C5F0C9332C");
        });

        modelBuilder.Entity<Item1Attribute>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Item1_At__3214EC27F063EC53");

            entity.HasOne(d => d.IdattributeNavigation).WithMany(p => p.Item1Attributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item1_Attribute_Attribute");

            entity.HasOne(d => d.Iditem1Navigation).WithMany(p => p.Item1Attributes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Item1_Attribute_Item1");
        });

        modelBuilder.Entity<Item2>(entity =>
        {
            entity.HasKey(e => e.Iditem2).HasName("PK__Item2__D4E826C6AFCD9854");
        });

        modelBuilder.Entity<Item3>(entity =>
        {
            entity.HasKey(e => e.Iditem3).HasName("PK__Item3__D4E826C72A401597");
        });

        modelBuilder.Entity<Item4>(entity =>
        {
            entity.HasKey(e => e.Iditem4).HasName("PK__Item4__D4E826C833A90520");
        });

        modelBuilder.Entity<Map>(entity =>
        {
            entity.HasKey(e => e.Idmap).HasName("PK__Map__9419593BDE1D66AA");
        });

        modelBuilder.Entity<MapMob>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Map_Mob__3214EC27537696A8");

            entity.HasOne(d => d.IdmapNavigation).WithMany(p => p.MapMobs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Map_Mob_Map");

            entity.HasOne(d => d.IdmobNavigation).WithMany(p => p.MapMobs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Map_Mob_Mob");
        });

        modelBuilder.Entity<MapNpc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Map_NPC__3214EC27DE5A4A53");

            entity.HasOne(d => d.IdmapNavigation).WithMany(p => p.MapNpcs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MapNPC_Map");

            entity.HasOne(d => d.IdnpcNavigation).WithMany(p => p.MapNpcs)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MapNPC_NPC");
        });

        modelBuilder.Entity<Mob>(entity =>
        {
            entity.HasKey(e => e.Idmob).HasName("PK__Mob__941E275E6DF61B15");
        });

        modelBuilder.Entity<Npc>(entity =>
        {
            entity.HasKey(e => e.Idnpc).HasName("PK__NPC__945ECD7AD3FDACEE");
        });

        modelBuilder.Entity<School>(entity =>
        {
            entity.HasKey(e => e.Idschool).HasName("PK__School__41E8DACC599235B0");

            entity.Property(e => e.Idschool).ValueGeneratedNever();
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Idskill).HasName("PK__Skill__E5D979F2AC42F3EF");

            entity.Property(e => e.Idskill).ValueGeneratedNever();

            entity.HasOne(d => d.IdschoolNavigation).WithMany(p => p.Skills).HasConstraintName("FK_Skill_School");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
