using System;
using System.Collections.Generic;
using VirtoCommerce.Platform.Core.Common;
using VirtoCommerce.Platform.Core.DynamicProperties;

namespace VirtoCommerce.InventoryModule.Core.Model
{
    public class FulfillmentCenter : AuditableEntity, IHasDynamicProperties, ICloneable, IHasOuterId
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string GeoLocation { get; set; }
        public Address Address { get; set; }
        public string OuterId { get; set; }
        public string OrganizationId { get; set; }

        public string ObjectType => typeof(FulfillmentCenter).FullName;

        public ICollection<DynamicObjectProperty> DynamicProperties { get; set; } = new List<DynamicObjectProperty>();

        #region ICloneable members

        public virtual object Clone()
        {
            var result = MemberwiseClone() as FulfillmentCenter;

            if (Address != null)
            {
                result.Address = Address.Clone() as Address;
            }

            return result;
        }

        #endregion
    }
}
