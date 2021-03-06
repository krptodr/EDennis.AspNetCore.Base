﻿// <auto-generated />
using System;
using EDennis.Samples.Hr.InternalApi2.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace EDennis.Samples.Hr.InternalApi2.Migrations
{
    [DbContext(typeof(AgencyInvestigatorCheckContext))]
    partial class AgencyInvestigatorCheckContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("EDennis.Samples.Hr.InternalApi2.Models.AgencyInvestigatorCheck", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("DateCompleted");

                    b.Property<int>("EmployeeId");

                    b.Property<string>("Status");

                    b.Property<DateTime>("SysEnd");

                    b.Property<DateTime>("SysStart");

                    b.Property<string>("SysUser");

                    b.Property<string>("SysUserNext");

                    b.HasKey("Id");

                    b.ToTable("AgencyInvestigatorCheck");
                });
#pragma warning restore 612, 618
        }
    }
}
