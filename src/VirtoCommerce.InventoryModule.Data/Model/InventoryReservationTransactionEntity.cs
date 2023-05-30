using System;
using VirtoCommerce.InventoryModule.Core.Model;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.Domain;

namespace VirtoCommerce.InventoryModule.Data.Model
{
    public class InventoryReservationTransactionEntity : AuditableEntity, IDataEntity<InventoryReservationTransactionEntity, InventoryReservationTransaction>
    {
        public string OuterId { get; set; }
        public string OuterType { get; set; }
        public string ProductId { get; set; }
        public string FulfillmentCenterId { get; set; }
        public string ParentId { get; set; }
        public int Type { get; set; }
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
            OuterId = transaction.OuterId;
            OuterType = transaction.OuterType;
            ProductId = transaction.ProductId;
            FulfillmentCenterId = transaction.FulfillmentCenterId;
            ParentId = transaction.ParentId;
            Type = (int)transaction.Type;
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
            transaction.OuterId = OuterId;
            transaction.OuterType = OuterType;
            transaction.ProductId = ProductId;
            transaction.FulfillmentCenterId = FulfillmentCenterId;
            transaction.ParentId = ParentId;
            transaction.Type = EnumUtility.SafeParse(Type.ToString(), TransactionType.Reservation);
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
