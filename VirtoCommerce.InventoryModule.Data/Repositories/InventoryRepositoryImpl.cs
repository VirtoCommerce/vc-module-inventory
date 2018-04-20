using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using VirtoCommerce.InventoryModule.Data.Model;
using VirtoCommerce.Platform.Data.Infrastructure;
using VirtoCommerce.Platform.Data.Infrastructure.Interceptors;

namespace VirtoCommerce.InventoryModule.Data.Repositories
{
    public class InventoryRepositoryImpl : EFRepositoryBase, IInventoryRepository
    {
        public InventoryRepositoryImpl()
        {
        }

        public InventoryRepositoryImpl(string nameOrConnectionString, params IInterceptor[] interceptors)
            : base(nameOrConnectionString, null, interceptors)
        {
            Configuration.LazyLoadingEnabled = false;
        }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<InventoryEntity>().ToTable("Inventory").HasKey(x => x.Id).Property(x => x.Id);

            modelBuilder.Entity<FulfillmentCenterEntity>().ToTable("FulfillmentCenter").HasKey(x => x.Id).Property(x => x.Id);
            modelBuilder.Entity<InventoryEntity>().HasRequired(x => x.FulfillmentCenter).WithMany()
                                                 .HasForeignKey(x => x.FulfillmentCenterId)
                                                 .WillCascadeOnDelete(true);


            base.OnModelCreating(modelBuilder);
        }


        #region IFoundationInventoryRepository Members

        public IQueryable<InventoryEntity> Inventories
        {
            get { return GetAsQueryable<InventoryEntity>(); }
        }
        public IQueryable<FulfillmentCenterEntity> FulfillmentCenters
        {
            get { return GetAsQueryable<FulfillmentCenterEntity>(); }
        }

        public IEnumerable<InventoryEntity> GetProductsInventories(IEnumerable<string> productIds)
        {
            return Inventories.Where(x => productIds.Contains(x.Sku)).Include(x => x.FulfillmentCenter).ToArray();
        }

        public IEnumerable<FulfillmentCenterEntity> GetFulfillmentCenters(IEnumerable<string> ids)
        {
            return FulfillmentCenters.Where(x => ids.Contains(x.Id)).ToArray();
        }

        #endregion
    }

}
