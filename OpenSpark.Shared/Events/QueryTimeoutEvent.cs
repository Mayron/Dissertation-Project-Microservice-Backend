namespace OpenSpark.Shared.Events
{
    public sealed class MultiQueryTimeout
    {
        public static MultiQueryTimeout Instance { get; } = new MultiQueryTimeout();

        private MultiQueryTimeout()
        {
        }
    }
}