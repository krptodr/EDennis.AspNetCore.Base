﻿// <auto-generated />
using EDennis.Samples.DefaultPoliciesApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EDennis.Samples.DefaultPoliciesApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20191008092258_Initial")]
    partial class Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "3.0.0");

            modelBuilder.Entity("EDennis.Samples.DefaultPoliciesApi.Models.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Person");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Moe"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Larry"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Curly"
                        });
                });

            modelBuilder.Entity("EDennis.Samples.DefaultPoliciesApi.Models.Position", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Position");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Title = "Manager"
                        },
                        new
                        {
                            Id = 2,
                            Title = "Employee"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
