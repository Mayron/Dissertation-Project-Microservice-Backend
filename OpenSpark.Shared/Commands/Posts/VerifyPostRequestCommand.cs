using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Posts
{
    public class VerifyPostRequestCommand : ICommand
    {
        public string GroupId { get; set; }
        public User User { get; set; }
    }
}
