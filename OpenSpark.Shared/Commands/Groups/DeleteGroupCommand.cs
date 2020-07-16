using System;
using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands.Groups
{
    public class DeleteGroupCommand : ICommand
    {
        public Guid TransactionId { get; set; }
        public string GroupId { get; set; }
        public User User { get; set; }
    }
}