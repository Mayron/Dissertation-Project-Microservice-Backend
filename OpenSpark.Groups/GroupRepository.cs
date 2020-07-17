using System.Collections.Generic;
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

        public List<Group> GetAllUserGroupsOrderedByContribution(string userId, int maxLimit)
        {
            using var session = DocumentStoreSingleton.Store.OpenSession();

            var  session.Query<Group>().Where(g => g.Members.Contains(userId));


            // 
        }
    }
}