using OpenSpark.Domain;
using System.Linq;

namespace OpenSpark.Groups
{
    public class GroupRepository
    {
        public Member GetGroupMemberByAuthUserId(string userId, string groupId)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            return session.Query<Member>().SingleOrDefault(
                g => g.AuthUserId == userId && g.GroupId == groupId);
        }
    }
}