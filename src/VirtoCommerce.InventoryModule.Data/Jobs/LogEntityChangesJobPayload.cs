using VirtoCommerce.Platform.Core.ChangeLog;

namespace VirtoCommerce.InventoryModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that persists inventory change-log entries.
    /// </summary>
    public class LogEntityChangesJobPayload
    {
        public OperationLog[] OperationLogs { get; set; }
    }
}
