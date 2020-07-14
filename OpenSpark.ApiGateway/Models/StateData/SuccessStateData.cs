using System;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public class SuccessStateData : BaseSagaStateData
    {
        public string Message { get; }

        public SuccessStateData(Guid transactionId, string message) : base(transactionId)
        {
            Message = message;
        }
    }
}