using System.Threading;
using System.Threading.Tasks;
using OpenSpark.Domain;

namespace OpenSpark.ApiGateway.Services.SDK
{
    public interface IFirestoreService
    {
        Task<User> GetUserAsync(string authId, CancellationToken cancellationToken);
    }
}
