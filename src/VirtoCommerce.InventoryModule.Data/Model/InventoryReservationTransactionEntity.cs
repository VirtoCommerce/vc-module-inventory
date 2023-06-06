using System;
using System.ComponentModel.DataAnnotations;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.InventoryModule.Data.Model
{
    public class InventoryReservationTransactionEntity : AuditableEntity, IDataEntity<InventoryReservationTransactionEntity, InventoryReservationTransaction>
    {
        [StringLength(128)]
        public string Type { get; set; }

        [StringLength(128)]
        public string ParentId { get; set; }

        [StringLength(256)]
        public string ItemType { get; set; }

        [StringLength(128)]
        public string ItemId { get; set; }

        [StringLength(128)]
        public string FulfillmentCenterId { get; set; }

        [StringLength(128)]
        public string ProductId { get; set; }

        public decimal Quantity { get; set; }

        public DateTime? ExpirationDate { get; set; }

        public InventoryReservationTransactionEntity FromModel(InventoryReservationTransaction transaction, PrimaryKeyResolvingMap pkMap)
        {
            pkMap.AddPair(transaction, this);

            Id = transaction.Id;
            CreatedBy = transaction.CreatedBy;
            CreatedDate = transaction.CreatedDate;
            ModifiedBy = transaction.ModifiedBy;
            ModifiedDate = transaction.ModifiedDate;
            Type = transaction.Type.ToString();
            ParentId = transaction.ParentId;
            ItemType = transaction.ItemType;
            ItemId = transaction.ItemId;
            FulfillmentCenterId = transaction.FulfillmentCenterId;
            ProductId = transaction.ProductId;
            Quantity = transaction.Quantity;
            ExpirationDate = transaction.ExpirationDate;

            return this;
        }

        public InventoryReservationTransaction ToModel(InventoryReservationTransaction transaction)
        {
            transaction.Id = Id;
            transaction.CreatedBy = CreatedBy;
            transaction.CreatedDate = CreatedDate;
            transaction.ModifiedBy = ModifiedBy;
            transaction.ModifiedDate = ModifiedDate;
            transaction.Type = EnumUtility.SafeParse(Type, TransactionType.Undefined);
            transaction.ParentId = ParentId;
            transaction.ItemType = ItemType;
            transaction.ItemId = ItemId;
            transaction.FulfillmentCenterId = FulfillmentCenterId;
            transaction.ProductId = ProductId;
            transaction.Quantity = (long)Quantity;
            transaction.ExpirationDate = ExpirationDate;

            return transaction;
        }

        public void Patch(InventoryReservationTransactionEntity target)
        {
            //Transaction isn't available for modifying
        }
    }
}
