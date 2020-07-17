namespace OpenSpark.Shared.Queries
{
    public interface IQuery : IMessage
    {
        string ConnectionId { get; set; }
        string Callback { get; set; }
    }
}