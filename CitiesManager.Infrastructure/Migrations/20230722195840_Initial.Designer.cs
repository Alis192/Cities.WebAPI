﻿// <auto-generated />
using System;
using CitiesManager.Infrastructure.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CitiesManager.Infrastructure.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20230722195840_Initial")]
    partial class Initial
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CitiesManager.Core.Entities.City", b =>
                {
                    b.Property<Guid>("CityID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("CityName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("CityID");

                    b.ToTable("Cities");

                    b.HasData(
                        new
                        {
                            CityID = new Guid("2d827f3b-d7ab-41f4-af69-995d4588ac7e"),
                            CityName = "Baku"
                        },
                        new
                        {
                            CityID = new Guid("3cb7f21d-553b-4222-b4d0-91c8d4e9dbdc"),
                            CityName = "London"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
