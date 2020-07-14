using System;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public class ErrorStateData : BaseSagaStateData
    {
        public string Message { get; }

        public ErrorStateData(Guid transactionId, string message) : base(transactionId)
        {
            Message = message;
        }
    }
}