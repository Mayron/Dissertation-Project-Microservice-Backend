namespace OpenSpark.Shared.Queries
{
    public interface IQuery : IMessage
    {
        string ConnectionId { get; set; }
    }
}