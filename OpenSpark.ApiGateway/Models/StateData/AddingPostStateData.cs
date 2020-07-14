using System;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public class AddingPostStateData : BaseSagaStateData
    {
        public AddingPostStateData(Guid transactionId) : base(transactionId)
        {
        }
    }
}