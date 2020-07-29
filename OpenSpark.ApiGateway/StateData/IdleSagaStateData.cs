using System;
using OpenSpark.Shared;

namespace OpenSpark.ApiGateway.StateData
{
    public class IdleSagaStateData : ISagaStateData
    {
        public static IdleSagaStateData Instance = new IdleSagaStateData();
        public Guid TransactionId { get; set; } = Guid.Empty;
        public MetaData MetaData { get; set; }
    }
}