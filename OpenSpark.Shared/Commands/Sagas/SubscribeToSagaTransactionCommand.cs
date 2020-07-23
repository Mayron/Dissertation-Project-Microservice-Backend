using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands
{
    public class SubscribeToSagaTransactionCommand : ICommand
    {
        public Guid TransactionId { get; set; }
        public string ConnectionId { get; set; }
        public string Callback { get; set; }
        public User User { get; set; }
    }
}