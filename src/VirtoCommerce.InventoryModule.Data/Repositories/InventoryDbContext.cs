using System.Reflection;
using Microsoft.EntityFrameworkCore;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.Platform.Data.Extensions;
using VirtoCommerce.Platform.Data.Infrastructure;

namespace VirtoCommerce.InventoryModule.Data.Repositories
{
    public class InventoryDbContext : DbContextBase
    {
        private const int _maxLength = 128;

#pragma warning disable S109
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
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FulfillmentCenterEntity>(builder =>
            {
                builder.ToAuditableEntityTable("FulfillmentCenter");
                builder.HasIndex(x => x.OuterId);
            });

            modelBuilder.Entity<InventoryEntity>(builder =>
            {
                builder.ToAuditableEntityTable("Inventory");
                builder.Property(x => x.BackorderQuantity).HasPrecision(18, 2);
                builder.Property(x => x.InStockQuantity).HasPrecision(18, 2);
                builder.Property(x => x.PreorderQuantity).HasPrecision(18, 2);
                builder.Property(x => x.ReorderMinQuantity).HasPrecision(18, 2);
                builder.Property(x => x.ReservedQuantity).HasPrecision(18, 2);

                builder.HasOne(x => x.FulfillmentCenter).WithMany()
                    .HasForeignKey(x => x.FulfillmentCenterId).IsRequired()
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasIndex(inv => new { inv.Sku, inv.ModifiedDate /* (! Important !) DESC */ }).IsUnique(false).HasDatabaseName("IX_Inventory_Sku_ModifiedDate");
                builder.HasIndex(x => x.OuterId);
            });

            modelBuilder.Entity<FulfillmentCenterDynamicPropertyObjectValueEntity>(builder =>
            {
                builder.ToAuditableEntityTable("FulfillmentCenterDynamicPropertyObjectValue");
                builder.Property(x => x.DecimalValue).HasColumnType("decimal(18,5)");

                builder.HasOne(p => p.FulfillmentCenter)
                    .WithMany(s => s.DynamicPropertyObjectValues).HasForeignKey(k => k.ObjectId)
                    .OnDelete(DeleteBehavior.Cascade);

                builder.HasIndex(x => new { x.ObjectType, x.ObjectId })
                    .IsUnique(false)
                    .HasDatabaseName("IX_FulfillmentCenterDynamicPropertyObjectValue_ObjectType_ObjectId");
            });

            modelBuilder.Entity<InventoryReservationTransactionEntity>(builder =>
            {
                builder.ToAuditableEntityTable("InventoryReservationTransaction");
                builder.Property(x => x.Quantity).HasPrecision(18, 2);
                builder.HasIndex(x => new { x.ItemId, x.FulfillmentCenterId, x.ItemType, x.Type }).IsUnique();
            });

            // Allows configuration for an entity type for different database types.
            // Applies configuration from all <see cref="IEntityTypeConfiguration{TEntity}" in VirtoCommerce.InventoryModule.Data.XXX project. /> 
            switch (Database.ProviderName)
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
