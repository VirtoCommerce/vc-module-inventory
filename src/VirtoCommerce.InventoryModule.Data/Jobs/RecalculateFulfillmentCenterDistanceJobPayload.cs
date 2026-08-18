namespace VirtoCommerce.InventoryModule.Data.Jobs
{
    /// <summary>
    /// Payload of the background job that refreshes the cached fulfillment center distances. Carries no data: the job
    /// recalculates everything, and the enqueue is a plain "the geo data changed" signal. It exists because the job
    /// API is payload-addressed, and it stays a class so a partner module can extend it via AbstractTypeFactory.
    /// </summary>
    public class RecalculateFulfillmentCenterDistanceJobPayload
    {
    }
}
