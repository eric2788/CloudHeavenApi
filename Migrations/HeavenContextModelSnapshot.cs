﻿// <auto-generated />
using System;
using CloudHeavenApi.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace CloudHeavenApi.Migrations
{
    [DbContext(typeof(HeavenContext))]
    partial class HeavenContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.1.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("CloudHeavenApi.Contexts.Badge", b =>
                {
                    b.Property<int>("BadgeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("BadgeLink")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("BadgeName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("BadgeId");

                    b.ToTable("CloudHeaven_Badge");
                });

            modelBuilder.Entity("CloudHeavenApi.Contexts.PersonBadges", b =>
                {
                    b.Property<Guid>("Uuid")
                        .HasColumnType("char(36)");

                    b.Property<int>("BadgeId")
                        .HasColumnType("int");

                    b.HasKey("Uuid", "BadgeId");

                    b.HasIndex("BadgeId");

                    b.ToTable("CloudHeaven_PersonBadges");
                });

            modelBuilder.Entity("CloudHeavenApi.Contexts.WebAccount", b =>
                {
                    b.Property<Guid>("Uuid")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("char(36)");

                    b.Property<bool>("Admin")
                        .HasColumnType("tinyint(1)");

                    b.Property<long>("JoinTime")
                        .HasColumnType("bigint");

                    b.Property<string>("NickName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("Status")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.Property<string>("UserName")
                        .HasColumnType("longtext CHARACTER SET utf8mb4");

                    b.HasKey("Uuid");

                    b.ToTable("CloudHeaven_WebAccount");
                });

            modelBuilder.Entity("CloudHeavenApi.Contexts.PersonBadges", b =>
                {
                    b.HasOne("CloudHeavenApi.Contexts.Badge", "Badge")
                        .WithMany("PersonBadgeses")
                        .HasForeignKey("BadgeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CloudHeavenApi.Contexts.WebAccount", "WebAccount")
                        .WithMany("PersonBadgeses")
                        .HasForeignKey("Uuid")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
