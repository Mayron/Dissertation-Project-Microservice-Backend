using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Sagas
{
    public class CreateAddPostRequestCommand : ISagaCommand
    {
        public Guid TransactionId { get; set; }
        public Post Post { get; set; }
        public string GroupId { get; set; }
        public User User { get; set; }
    }
}
