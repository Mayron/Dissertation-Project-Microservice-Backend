using System;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public class ErrorStateData : BaseSagaStateData
    {
        public ISagaStateData PreviousStateData { get; }
        public string Message { get; }

        public ErrorStateData(ISagaStateData previousStateData, string message) : base(previousStateData.TransactionId)
        {
            PreviousStateData = previousStateData;
            Message = message;
        }
    }
}