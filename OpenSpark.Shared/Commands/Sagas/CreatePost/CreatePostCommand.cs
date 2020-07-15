using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Sagas.CreatePost
{
    public class CreatePostCommand : ISagaCommand
    {
        public Post Post { get; set; }
        public string GroupId { get; set; }
        public Guid TransactionId { get; set; }

        public CreatePostCommand()
        {
        }
    }
}
