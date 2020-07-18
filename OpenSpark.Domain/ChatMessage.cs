using System;

namespace OpenSpark.Domain
{
    public class ChatMessage
    {
        public DateTime TimeStamp { get; set; }
        public string Body { get; set; }
        public string Author { get; set; }
        public Guid Id { get; set; }
    }
}
