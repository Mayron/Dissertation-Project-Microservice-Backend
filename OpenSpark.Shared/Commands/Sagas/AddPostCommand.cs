using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Sagas
{
    public class AddPostCommand : ISagaCommand
    {
        public Post Post { get; set; }
        public string GroupId { get; set; }
        public Guid TransactionId { get; set; }

        public AddPostCommand()
        {
        }
    }
}
