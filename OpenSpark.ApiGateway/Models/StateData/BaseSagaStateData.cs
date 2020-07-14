using System;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public class BaseSagaStateData : ISagaStateData
    {
        public Guid TransactionId { get; set; }

        public BaseSagaStateData(Guid transactionId)
        {
            TransactionId = transactionId;
        }
    }
}