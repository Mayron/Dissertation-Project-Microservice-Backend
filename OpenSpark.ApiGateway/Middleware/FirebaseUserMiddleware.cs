using System.Threading;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Services;
using System.Threading.Tasks;

namespace OpenSpark.ApiGateway.Middleware
{
    public class FirebaseUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IFirestoreService _firestoreService;

        public FirebaseUserMiddleware(RequestDelegate next, IFirestoreService firestoreService)
        {
            _next = next;
            _firestoreService = firestoreService;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.User != null && httpContext.User.Identity.IsAuthenticated)
            {
                var use = await _firestoreService.GetUserAsync(httpContext.User, CancellationToken.None);
            }

            await _next(httpContext);
        }
    }
}