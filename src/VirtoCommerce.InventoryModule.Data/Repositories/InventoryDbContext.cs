using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.InventoryModule.Data.Model;

namespace VirtoCommerce.InventoryModule.Data.Repositories
{
    public class InventoryDbContext : DbContextWithTriggers
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options)
            : base(options)
        {
        }

        protected InventoryDbContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryEntity>().ToTable("Inventory").HasKey(x => x.Id);
            modelBuilder.Entity<InventoryEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<InventoryEntity>().HasIndex(inv => new { inv.Sku, inv.ModifiedDate /* (! Important !) DESC */ }).IsUnique(false).HasDatabaseName("IX_Inventory_Sku_ModifiedDate");
            modelBuilder.Entity<InventoryEntity>().HasOne(x => x.FulfillmentCenter).WithMany()
                .HasForeignKey(x => x.FulfillmentCenterId).IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<InventoryEntity>().Property(x => x.BackorderQuantity).HasPrecision(18, 2);
            modelBuilder.Entity<InventoryEntity>().Property(x => x.InStockQuantity).HasPrecision(18, 2);
            modelBuilder.Entity<InventoryEntity>().Property(x => x.PreorderQuantity).HasPrecision(18, 2);
            modelBuilder.Entity<InventoryEntity>().Property(x => x.ReorderMinQuantity).HasPrecision(18, 2);
            modelBuilder.Entity<InventoryEntity>().Property(x => x.ReservedQuantity).HasPrecision(18, 2);

            modelBuilder.Entity<FulfillmentCenterEntity>().ToTable("FulfillmentCenter").HasKey(x => x.Id);
            modelBuilder.Entity<FulfillmentCenterEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();

            base.OnModelCreating(modelBuilder);
        }
    }
}
