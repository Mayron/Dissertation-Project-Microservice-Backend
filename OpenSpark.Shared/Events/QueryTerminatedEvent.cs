using OpenSpark.Shared.Events.Payloads;

namespace OpenSpark.Shared.Events
{
    public sealed class QueryTerminatedEvent : PayloadEvent
    {
        public static QueryTerminatedEvent Instance { get; } = new QueryTerminatedEvent();

        private QueryTerminatedEvent()
        {
        }
    }
}