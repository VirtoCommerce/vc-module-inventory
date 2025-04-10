using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VirtoCommerce.Platform.Core.Settings;

namespace VirtoCommerce.InventoryModule.Core
{
    [ExcludeFromCodeCoverage]
    public static class ModuleConstants
    {
        public static class Security
        {
            public static class Permissions
            {
                public const string FulfillmentRead = "inventory:fulfillment:read";
                public const string FulfillmentEdit = "inventory:fulfillment:edit";
                public const string FulfillmentDelete = "inventory:fulfillment:delete";
                public const string Read = "inventory:read";
                public const string Create = "inventory:create";
                public const string Update = "inventory:update";
                public const string Access = "inventory:access";
                public const string Delete = "inventory:delete";

                public static string[] AllPermissions { get; } = [
                    FulfillmentRead, FulfillmentEdit, FulfillmentDelete,
                    Read, Create, Update, Access, Delete,
                ];
            }
        }

        public static class Settings
        {
            public static class General
            {
                public static SettingDescriptor PageSize { get; } = new SettingDescriptor
                {
                    Name = "Inventory.ExportImport.PageSize",
                    GroupName = "Inventory | General",
                    ValueType = SettingValueType.PositiveInteger,
                    DefaultValue = 50,
                };

                public static SettingDescriptor LogInventoryChanges { get; } = new SettingDescriptor
                {
                    Name = "Inventory.LogInventoryChanges",
                    GroupName = "Inventory | General",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false,
                };

                public static IEnumerable<SettingDescriptor> AllSettings { get; } = new[]
                {
                    PageSize,
                    LogInventoryChanges
                };
            }

            public static class Search
            {
                public static SettingDescriptor EventBasedIndexationEnable { get; } = new SettingDescriptor
                {
                    Name = "Inventory.Search.EventBasedIndexation.Enable",
                    GroupName = "Inventory | Search",
                    ValueType = SettingValueType.Boolean,
                    DefaultValue = false
                };

                public static IEnumerable<SettingDescriptor> AllSettings { get; } = new[] { EventBasedIndexationEnable };
            }

            public static IEnumerable<SettingDescriptor> AllSettings
            {
                get
                {
                    return General.AllSettings.Concat(Search.AllSettings);
                }
            }
        }
    }
}
