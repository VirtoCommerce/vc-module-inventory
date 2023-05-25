using System.Reflection;
using EntityFrameworkCore.Triggers;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.InventoryModule.Data.Model;

namespace VirtoCommerce.InventoryModule.Data.Repositories
{
    public class InventoryDbContext : DbContextWithTriggers
    {
#pragma warning disable S109
        private const int MaxLength = 128;
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

            modelBuilder.Entity<FulfillmentCenterDynamicPropertyObjectValueEntity>().ToTable("FulfillmentCenterDynamicPropertyObjectValue").HasKey(x => x.Id);
            modelBuilder.Entity<FulfillmentCenterDynamicPropertyObjectValueEntity>().Property(x => x.Id).HasMaxLength(128).ValueGeneratedOnAdd();
            modelBuilder.Entity<FulfillmentCenterDynamicPropertyObjectValueEntity>().Property(x => x.DecimalValue).HasColumnType("decimal(18,5)");
            modelBuilder.Entity<FulfillmentCenterDynamicPropertyObjectValueEntity>().HasOne(p => p.FulfillmentCenter)
                .WithMany(s => s.DynamicPropertyObjectValues).HasForeignKey(k => k.ObjectId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<FulfillmentCenterDynamicPropertyObjectValueEntity>().HasIndex(x => new { x.ObjectType, x.ObjectId })
                .IsUnique(false)
                .HasDatabaseName("IX_FulfillmentCenterDynamicPropertyObjectValue_ObjectType_ObjectId");

            modelBuilder.Entity<InventoryReservationTransactionEntity>().HasKey(x => x.Id);
            modelBuilder.Entity<InventoryReservationTransactionEntity>().Property(x => x.Id).HasMaxLength(MaxLength).ValueGeneratedOnAdd();
            modelBuilder.Entity<InventoryReservationTransactionEntity>().Property(x => x.Quantity).HasPrecision(18, 2);
            modelBuilder.Entity<InventoryReservationTransactionEntity>().HasIndex(x => new { x.OuterId, x.OuterType, x.FulfillmentCenterId, x.Type }).IsUnique();
            modelBuilder.Entity<InventoryReservationTransactionEntity>().ToTable("InventoryReservationTransaction");

            base.OnModelCreating(modelBuilder);

            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.InventoryModule.Data.XXX project. /> 
            switch (this.Database.ProviderName)
            {
                case "Pomelo.EntityFrameworkCore.MySql":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.InventoryModule.Data.MySql"));
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.InventoryModule.Data.PostgreSql"));
                    break;
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.Load("VirtoCommerce.InventoryModule.Data.SqlServer"));
                    break;
            }

        }
#pragma warning restore S109
    }
}
