using System;

namespace OpenSpark.Shared.Commands.Sagas
{
    public class SubscribeToSagaTransactionCommand : ISagaCommand
    {
        public Guid TransactionId { get; set; }
        public string ConnectionId { get; set; }
        public string Callback { get; set; }
    }
}