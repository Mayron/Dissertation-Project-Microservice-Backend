using System;

namespace OpenSpark.ApiGateway.Models.StateData
{
    public class VerifyingUserStateData : BaseSagaStateData
    {
        public VerifyingUserStateData(Guid transactionId) : base(transactionId)
        {
        }
    }
}