using Microsoft.EntityFrameworkCore;
using DynamicObjectAPI.Domain.Entities;

namespace DynamicObjectAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceLine> InvoiceLines { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderProduct> OrderProducts { get; set; }
        public DbSet<ObjectType> ObjectTypes { get; set; }
        public DbSet<Field> Fields { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Order>()
                .Property(o => o.OrderDate)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder.Entity<Invoice>()
                .Property(i => i.InvoiceDate)
                .HasDefaultValueSql("NOW() AT TIME ZONE 'UTC'");

            modelBuilder.Entity<Customer>(entity =>
            {
                
            });

            modelBuilder.Entity<Invoice>(entity =>
            {
                entity.HasMany(i => i.InvoiceLines)
                    .WithOne(il => il.Invoice)
                    .HasForeignKey(il => il.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasMany(o => o.OrderProducts)
                    .WithOne(op => op.Order)
                    .HasForeignKey(op => op.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasMany(p => p.OrderProducts)
                    .WithOne(op => op.Product)
                    .HasForeignKey(op => op.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(p => p.InvoiceLines)
                    .WithOne(il => il.Product)
                    .HasForeignKey(il => il.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ObjectType>(entity =>
            {
                entity.HasMany(ot => ot.Fields)
                    .WithOne(f => f.ObjectType)
                    .HasForeignKey(f => f.ObjectTypeId);

                entity.HasOne(ot => ot.ParentObjectType)
                    .WithMany()
                    .HasForeignKey(ot => ot.ParentObjectTypeId);
            });
        }

        public override int SaveChanges()
        {
            SetTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void SetTimestamps()
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity &&
                    (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entry in entries)
            {
                var entity = (BaseEntity)entry.Entity;
                var now = DateTime.UtcNow;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt = now;
                    
                }
                else if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = now;
                }
            }
        }



    }
}