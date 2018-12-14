﻿// <auto-generated />
using System;
using EDennis.Samples.InternalApi2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EDennis.Samples.InternalApi2.Migrations
{
    [DbContext(typeof(AgencyInvestigatorCheckContext))]
    [Migration("20181214120445_AgencyInvestigatorCheck_Initial")]
    partial class AgencyInvestigatorCheck_Initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.0-rtm-35687")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("EDennis.Samples.InternalApi2.Models.AgencyInvestigatorCheck", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCompleted")
                        .HasColumnType("date");

                    b.Property<int>("EmployeeId");

                    b.Property<string>("Status")
                        .HasMaxLength(100);

                    b.Property<DateTime>("SysEnd")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("(convert(datetime2,'9999-12-31 23:59:59.9999999'))");

                    b.Property<DateTime>("SysStart")
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("datetime2")
                        .HasDefaultValueSql("(getdate())");

                    b.HasKey("Id");

                    b.ToTable("AgencyInvestigatorCheck");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            DateCompleted = new DateTime(2018, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            EmployeeId = 1,
                            Status = "Pass",
                            SysEnd = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            SysStart = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Id = 2,
                            DateCompleted = new DateTime(2018, 2, 2, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            EmployeeId = 2,
                            Status = "Pass",
                            SysEnd = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            SysStart = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Id = 3,
                            DateCompleted = new DateTime(2018, 3, 3, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            EmployeeId = 3,
                            Status = "Fail",
                            SysEnd = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            SysStart = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        },
                        new
                        {
                            Id = 4,
                            DateCompleted = new DateTime(2018, 4, 4, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            EmployeeId = 4,
                            Status = "Pass",
                            SysEnd = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                            SysStart = new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified)
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
