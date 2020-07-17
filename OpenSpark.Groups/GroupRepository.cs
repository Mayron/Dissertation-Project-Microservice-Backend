using OpenSpark.Domain;
using System.Linq;

namespace OpenSpark.Groups
{
    public class GroupRepository
    {
        public Member GetGroupMember(string userId, string groupId)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            return session.Query<Member>().SingleOrDefault(
                g => g.UserId == userId && g.GroupId == groupId);
        }
    }
}