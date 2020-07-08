using OpenSpark.Domain;

namespace OpenSpark.Discussions.Commands
{
    public class ConnectUserCommand
    {
        public string ConnectionId { get; set; }
        public User User { get; set; }
    }
}
