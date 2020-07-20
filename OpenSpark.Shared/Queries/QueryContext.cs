namespace OpenSpark.Shared.Queries
{
    public class QueryContext
    {
        public int RemoteSystemId { get; set; }
        public IQuery Query { get; set; }
    }
}