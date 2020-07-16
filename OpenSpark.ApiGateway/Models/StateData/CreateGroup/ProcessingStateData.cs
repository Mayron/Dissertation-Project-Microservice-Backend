using OpenSpark.Domain;
using System;
using System.Collections.Generic;

namespace OpenSpark.ApiGateway.Models.StateData.CreateGroup
{
    public class ProcessingStateData : ISagaStateData
    {
        public Guid TransactionId { get; set; }
        public User User { get; set; }
        public List<string> Connecting { get; set; }
        public int SuccessfulConnections { get; set; }
        public int FailedConnections { get; set; }
    }
}