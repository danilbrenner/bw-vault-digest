﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using VaultSummary.Data;

#nullable disable

namespace VaultSummary.Data.Migrations
{
    [DbContext(typeof(VaultSummaryContext))]
    [Migration("20240612091023_Init")]
    partial class Init
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("VaultSummary.Data.DataModel.Login", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(300)
                        .HasColumnType("nvarchar(300)");

                    b.HasKey("Id");

                    b.ToTable("Logins", (string)null);
                });

            modelBuilder.Entity("VaultSummary.Data.DataModel.Password", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Hash")
                        .IsRequired()
                        .HasMaxLength(80)
                        .HasColumnType("nvarchar(80)");

                    b.Property<DateTimeOffset>("LastUsedDate")
                        .HasColumnType("datetimeoffset");

                    b.Property<Guid>("LoginId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<byte>("Strength")
                        .HasColumnType("tinyint");

                    b.HasKey("Id");

                    b.HasIndex("LoginId");

                    b.ToTable("Passwords", (string)null);
                });

            modelBuilder.Entity("VaultSummary.Data.DataModel.Password", b =>
                {
                    b.HasOne("VaultSummary.Data.DataModel.Login", null)
                        .WithMany("Passwords")
                        .HasForeignKey("LoginId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("VaultSummary.Data.DataModel.Login", b =>
                {
                    b.Navigation("Passwords");
                });
#pragma warning restore 612, 618
        }
    }
}
