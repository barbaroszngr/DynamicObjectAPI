using Microsoft.EntityFrameworkCore;
using DynamicObjectAPI.Domain.Entities;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicObjectAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<DynamicObject> DynamicObjects { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DynamicObject>(entity =>
            {
                entity.ToTable("DynamicObjects");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.ObjectType)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(e => e.Data)
                      .HasColumnType("jsonb")
                      .IsRequired();

                entity.Property(e => e.ParentId);

                entity.Property(e => e.CreatedAt)
                      .IsRequired()
                      .HasDefaultValueSql("NOW()");

                entity.Property(e => e.UpdatedAt);

                entity.Property(e => e.CustomerId);

                entity.HasIndex(e => e.CustomerId)
                      .HasFilter("\"CustomerId\" IS NOT NULL");

            });
        }
    }
}
