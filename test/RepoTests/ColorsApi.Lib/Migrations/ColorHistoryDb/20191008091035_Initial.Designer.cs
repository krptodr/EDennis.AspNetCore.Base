﻿// <auto-generated />
using System;
using EDennis.Samples.Colors.InternalApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EDennis.Samples.Colors.InternalApi.Migrations.ColorHistoryDb
{
    [DbContext(typeof(ColorHistoryDbContext))]
    [Migration("20191008091035_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("EDennis.Samples.Colors.InternalApi.Models.Color", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnType("int");

                    b.Property<DateTime>("SysStart")
                        .HasColumnType("datetime2");

                    b.Property<string>("Name")
                        .HasColumnType("varchar(30)")
                        .HasMaxLength(30)
                        .IsUnicode(false);

                    b.Property<DateTime>("SysEnd")
                        .HasColumnType("datetime2");

                    b.Property<string>("SysUser")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("SysUserNext")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id", "SysStart");

                    b.ToTable("Color","dbo_history");
                });
#pragma warning restore 612, 618
        }
    }
}
