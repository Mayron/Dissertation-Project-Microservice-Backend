namespace OpenSpark.Shared.Events.Payloads
{
    public sealed class PayloadTimedOutEvent : IPayloadEvent
    {
        public static PayloadTimedOutEvent Instance { get; } = new PayloadTimedOutEvent();

        private PayloadTimedOutEvent() {}
    }
}