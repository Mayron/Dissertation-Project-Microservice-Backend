using System;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public class SagaStateData : ISagaStateData
    {
        public Guid TransactionId { get; set; }

        public SagaStateData(Guid transactionId)
        {
            TransactionId = transactionId;
        }
    }
}