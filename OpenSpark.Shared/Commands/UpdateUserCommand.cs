using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands
{
    public class UpdateUserCommand
    {
        public string AuthUserId { get; set; }
        public Group[] GroupsToAdd { get; set; }
    }
}