using System;

namespace OpenSpark.Shared.Events.Sagas
{
    public interface ISagaEvent
    {
        Guid TransactionId { get; set; }
    }
}