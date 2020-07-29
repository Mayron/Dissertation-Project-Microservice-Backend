namespace OpenSpark.Domain
{
    public class Vote
    {
        public string UserId { get; set; }
        public bool Up { get; set; }
        public bool Down { get; set; }
    }
}