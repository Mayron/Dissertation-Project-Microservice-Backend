using OpenSpark.Domain;
using Raven.Client.Documents.Indexes;
using System.Linq;

namespace OpenSpark.Groups.Indexes
{
    public class GetBasicGroupDetails : AbstractIndexCreationTask<Group, GetBasicGroupDetails.Result>
    {
        public class Result
        {
            public string About { get; set; }
            public string CategoryId { get; set; }
            public string GroupId { get; set; }
            public string Name { get; set; }
            public string Visibility { get; set; }
        }

        public GetBasicGroupDetails()
        {
            Map = groups => from g in groups
                            select new
                            {
                                g.About,
                                g.CategoryId,
                                g.GroupId,
                                g.Name,
                                g.Visibility,
                            };
        }
    }
}