using OpenSpark.Shared.Commands.SagaExecutionCommands;
using System;

namespace OpenSpark.ApiGateway.Builders
{
    public class SagaContext
    {
        public ISagaExecutionCommand Command { get; set; }
        public Guid TransactionId { get; set; }
        public string SagaName { get; set; }
    }
}