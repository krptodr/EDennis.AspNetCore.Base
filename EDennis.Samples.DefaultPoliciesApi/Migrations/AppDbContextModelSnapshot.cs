﻿// <auto-generated />
using EDennis.Samples.DefaultPoliciesApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EDennis.Samples.DefaultPoliciesApi.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079");

            modelBuilder.Entity("EDennis.Samples.DefaultPoliciesApi.Models.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

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
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Title");

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
