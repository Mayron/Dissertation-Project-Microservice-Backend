using OpenSpark.Shared.Domain;

namespace OpenSpark.Teams.Domain
{
    public class Member : IEntity
    {
        public string Id { get; set; }
        public int Contributions { get; set; }
        public string TeamId { get; set; }
        public string UserAuthId { get; set; }
    }
}