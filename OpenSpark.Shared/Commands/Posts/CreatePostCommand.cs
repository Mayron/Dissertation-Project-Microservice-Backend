using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Posts
{
    public class CreatePostCommand : ICommand
    {
        public Post Post { get; set; }
        public string GroupId { get; set; }
        public Guid TransactionId { get; set; }
        public User User { get; set; }
    }
}
