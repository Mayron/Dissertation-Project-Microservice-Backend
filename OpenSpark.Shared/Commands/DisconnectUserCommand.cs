namespace OpenSpark.Shared.Commands
{
    public class DisconnectUserCommand : ICommand
    {
        public string ConnectionId { get; set; }
    }
}
