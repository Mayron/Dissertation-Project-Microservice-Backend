using OpenSpark.Domain;

namespace OpenSpark.Shared.Commands
{
    public class ConnectUserCommand : ICommand
    {
        public string ConnectionId { get; set; }
        public User User { get; set; }
    }
}
