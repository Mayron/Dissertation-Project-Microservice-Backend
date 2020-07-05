using System.Collections.Generic;
using System.Threading.Tasks;
using OpenSpark.ApiGateway.ViewModels;

namespace OpenSpark.ApiGateway.Services.SDK
{
    public interface INewsFeedService
    {
        Task<List<PostViewModel>> GetPostsAsync();
    }
}