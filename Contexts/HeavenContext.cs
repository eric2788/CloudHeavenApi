using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace CloudHeavenApi.Contexts
{
    public class HeavenContext : DbContext
    {
        public HeavenContext(DbContextOptions<HeavenContext> options) : base(options)
        {
        }

        public DbSet<WebAccount> WebAccounts { get; set; }
        public DbSet<PersonBadges> PersonBadges { get; set; }
        public DbSet<Badge> Badges { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Badge>().Property(b => b.BadgeId).ValueGeneratedOnAdd();
            modelBuilder.Entity<PersonBadges>().HasKey(s => new {s.Uuid, s.BadgeId});
            modelBuilder.Entity<PersonBadges>().HasOne(s => s.Badge).WithMany(b => b.PersonBadgeses)
                .HasForeignKey(b => b.BadgeId);
            modelBuilder.Entity<PersonBadges>().HasOne(s => s.WebAccount).WithMany(w => w.PersonBadgeses)
                .HasForeignKey(b => b.Uuid);
        }
    }

    [Table("CloudHeaven_WebAccount")]
    public class WebAccount
    {
        [Key] public Guid Uuid { get; set; }

        public string UserName { get; set; }

        public string NickName { get; set; }

        public string Status { get; set; }
        public long JoinTime { get; set; }

        public bool Admin { get; set; } = false;

        [JsonIgnore]
        public virtual ICollection<PersonBadges> PersonBadgeses { get; set; }
    }

    [Table("CloudHeaven_PersonBadges")]
    public class PersonBadges
    {
        public Guid Uuid { get; set; }
        public int BadgeId { get; set; }

        [JsonIgnore]
        [ForeignKey("Uuid")] public virtual WebAccount WebAccount { get; set; }

        [JsonIgnore]
        [ForeignKey("BadgeId")] public virtual Badge Badge { get; set; }
    }

    [Table("CloudHeaven_Badge")]
    public class Badge
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int BadgeId { get; set; }

        public string BadgeName { get; set; }
        public string BadgeLink { get; set; }

        [JsonIgnore]
        public virtual ICollection<PersonBadges> PersonBadgeses { get; set; }
    }
}