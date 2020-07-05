using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenSpark.ApiGateway.Services.SDK;
using OpenSpark.ApiGateway.ViewModels;

namespace OpenSpark.ApiGateway.Handlers
{
    public class NewsFeedPosts
    {
        public class Query : IRequest<List<PostViewModel>>
        {
        }

        public class Handler : IRequestHandler<Query, List<PostViewModel>>
        {
            private readonly INewsFeedService _newsFeedService;

            public Handler(INewsFeedService newsFeedService)
            {
                _newsFeedService = newsFeedService;
                // inject services
            }

            public async Task<List<PostViewModel>> Handle(Query request, CancellationToken cancellationToken)
            {
                return await _newsFeedService.GetPostsAsync();
            }
        }
    }
} 
