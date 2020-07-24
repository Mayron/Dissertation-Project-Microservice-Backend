using System;

namespace OpenSpark.ApiGateway.StateData
{
    public class IdleStateData : ISagaStateData
    {
        public static IdleStateData Instance = new IdleStateData();
        public Guid TransactionId { get; set; } = Guid.Empty;
    }
}