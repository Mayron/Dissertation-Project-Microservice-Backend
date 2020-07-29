using System;
using OpenSpark.Shared.Commands.Sagas;

namespace OpenSpark.ApiGateway.Builders
{
    public class SagaContext
    {
        public ISagaExecutionCommand Command { get; set; }
        public Guid Id { get; set; }
        public string SagaName { get; set; }
    }
}