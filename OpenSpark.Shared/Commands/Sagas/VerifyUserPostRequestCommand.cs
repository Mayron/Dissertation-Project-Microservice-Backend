using System;

namespace OpenSpark.Shared.Commands.Sagas
{
    public class VerifyUserPostRequestCommand : ISagaCommand
    {
        public Guid TransactionId { get; set; }
        public string UserId { get; set; }
        public string GroupId { get; set; }
    }
}
