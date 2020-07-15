using System;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public class IdleStateData : ISagaStateData
    {
        public static IdleStateData Instance = new IdleStateData();
        public Guid TransactionId { get; set; } = Guid.Empty;
    }
}