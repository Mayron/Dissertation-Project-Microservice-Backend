using OpenSpark.Domain;
using Raven.Client.Documents.Indexes;
using System.Linq;

namespace OpenSpark.Groups.Indexes
{
    public class GetBasicGroupDetails : AbstractMultiMapIndexCreationTask<GetBasicGroupDetails.Result>
    {
        public class Result
        {
            public string About { get; set; }
            public string CategoryId { get; set; }
            public string GroupId { get; set; }
            public string Name { get; set; }
            public string Visibility { get; set; }
            public int TotalMembers { get; set; }
        }

        public GetBasicGroupDetails()
        {
            AddMap<Group>(groups => 
                from g in groups
                select new
                {
                    g.About,
                    g.CategoryId,
                    g.GroupId,
                    g.Name,
                    g.Visibility,
                    TotalMembers = 0
                });

            AddMap<Member>(members => 
                from m in members
                select new
                {
                    About = "",
                    CategoryId = "",
                    m.GroupId,
                    Name = "",
                    Visibility = "",
                    TotalMembers = 1,
                });

            Reduce = results => from r in results
                group r by new {r.GroupId}
                into g
                select new
                {
                    About = g.Select(x => x.About).FirstOrDefault(),
                    CategoryId = g.Select(x => x.CategoryId).FirstOrDefault(),
                    g.Key.GroupId,
                    Name = g.Select(x => x.Name).FirstOrDefault(),
                    Visibility = g.Select(x => x.Visibility).FirstOrDefault(),
                    TotalMembers = g.Sum(x => x.TotalMembers)
                };
        }
    }
}