using OpenSpark.Shared;
using System;

namespace OpenSpark.ApiGateway.StateData
{
    public interface ISagaStateData
    {
        Guid TransactionId { get; set; }
        MetaData MetaData { get; set; }
    }
}