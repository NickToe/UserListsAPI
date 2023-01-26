﻿// <auto-generated />
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using UserListsAPI.DataLayer;

#nullable disable

namespace UserListsAPI.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20230126191200_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("UserListsAPI.DataLayer.Entities.Game", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<bool?>("ComingSoon")
                        .HasColumnType("boolean");

                    b.Property<List<string>>("Developers")
                        .HasColumnType("text[]");

                    b.Property<List<string>>("Genres")
                        .HasColumnType("text[]");

                    b.Property<int>("ItemStatus")
                        .HasColumnType("integer");

                    b.Property<short?>("MetacriticScore")
                        .HasColumnType("smallint");

                    b.Property<string>("MetacriticUrl")
                        .HasColumnType("text");

                    b.Property<string>("Poster")
                        .HasColumnType("text");

                    b.Property<List<string>>("Publishers")
                        .HasColumnType("text[]");

                    b.Property<string>("ReleaseDate")
                        .HasColumnType("text");

                    b.Property<string>("ReviewScore")
                        .HasColumnType("text");

                    b.Property<string>("ShortDescription")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int?>("TotalNegative")
                        .HasColumnType("integer");

                    b.Property<int?>("TotalPositive")
                        .HasColumnType("integer");

                    b.Property<int?>("TotalReviews")
                        .HasColumnType("integer");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("Title");

                    b.ToTable("Games");
                });

            modelBuilder.Entity("UserListsAPI.DataLayer.Entities.Movie", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("text");

                    b.Property<string>("Companies")
                        .HasColumnType("text");

                    b.Property<string>("ContentRating")
                        .HasColumnType("text");

                    b.Property<string>("Countries")
                        .HasColumnType("text");

                    b.Property<string>("Directors")
                        .HasColumnType("text");

                    b.Property<string>("FullTitle")
                        .HasColumnType("text");

                    b.Property<string>("Genres")
                        .HasColumnType("text");

                    b.Property<string>("ImdbRating")
                        .HasColumnType("text");

                    b.Property<string>("ImdbRatingVotes")
                        .HasColumnType("text");

                    b.Property<int>("ItemStatus")
                        .HasColumnType("integer");

                    b.Property<string>("MetascriticRating")
                        .HasColumnType("text");

                    b.Property<string>("Plot")
                        .HasColumnType("text");

                    b.Property<string>("Poster")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("ReleaseDate")
                        .HasColumnType("text");

                    b.Property<string>("RuntimeMins")
                        .HasColumnType("text");

                    b.Property<string>("RuntimeStr")
                        .HasColumnType("text");

                    b.Property<string>("Stars")
                        .HasColumnType("text");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Type")
                        .HasColumnType("text");

                    b.Property<string>("Year")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("Id");

                    b.HasIndex("Title");

                    b.ToTable("Movies");
                });
#pragma warning restore 612, 618
        }
    }
}
