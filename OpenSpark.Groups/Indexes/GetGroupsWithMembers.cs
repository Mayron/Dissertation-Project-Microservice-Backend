using System.Linq;
using OpenSpark.Domain;
using Raven.Client.Documents.Indexes;

namespace OpenSpark.Groups.Indexes
{
    public class GetGroupsWithMembers : AbstractMultiMapIndexCreationTask<GetGroupsWithMembers.Result>
    {
        public class Result
        {
            public string GroupId { get; set; }
            public string Name { get; set; }
            public string OwnerUserId { get; set; }
            public string MemberId { get; set; }
            public int Contribution { get; set; }
        }

        public GetGroupsWithMembers()
        {
            AddMap<Group>(groups => 
                from g in groups
                select new
                {
                    g.GroupId,
                    g.Name,
                    g.OwnerUserId,
                    MemberId = "",
                    Contribution = 0,
                });

            AddMap<Member>(members => 
                from m in members
                select new
                {
                    GroupId = "",
                    Name = "",
                    OwnerUserId = "",
                    MemberId = m.UserId,
                    m.Contribution,
                });

            Reduce = results => from r in results
                group r by new { r.GroupId }
                into g
                select new
                {
                    About = g.Select(x => x.About).FirstOrDefault(),
                    CategoryId = g.Select(x => x.CategoryId).FirstOrDefault(),
                    g.Key.GroupId,
                    Name = g.Select(x => x.Name).FirstOrDefault(),
                    Visibility = g.Select(x => x.MemberId).ToList(),
                    Contribution = g.Sum(x => x.Contribution)
                };

//        public string GroupId { get; set; }
//        public string Name { get; set; }
//        public string OwnerUserId { get; set; }
//        public string MemberId { get; set; }
//        public int Contribution { get; set; }
    }
    }
}