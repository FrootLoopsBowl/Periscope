using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Persistence.Migrations
{
    [DbContext(typeof(GarneauTemplateDbContext))]
    [Migration("20260315232306_AddPleasureToAthleteEffort")]
    partial class AddPleasureToAthleteEffort
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "8.0.2");

            modelBuilder.Entity("Domain.Entities.AthleteEffort", b =>
            {
                b.Property<int>("Pleasure")
                    .HasColumnType("int")
                    .HasColumnName("Pleasure");

                b.ToTable("AthleteEfforts");
            });
#pragma warning restore 612, 618
        }
    }
}
