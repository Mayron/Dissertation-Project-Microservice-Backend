using System;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public interface ISagaStateData
    {
        Guid TransactionId { get; set; }
    }
}
