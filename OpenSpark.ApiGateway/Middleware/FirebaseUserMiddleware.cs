using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using OpenSpark.ApiGateway.Services;
using System.Threading;
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
                try
                {
                    var authId = httpContext.User.Claims.Single(c => c.Type == "user_id").Value;
                    var user = await _firestoreService.GetUserAsync(authId, CancellationToken.None);

                    if (httpContext.User.HasClaim(c => c.Type == "email_verified"))
                    {
                        var claim = httpContext.User.Claims.Single(c => c.Type == "email_verified").Value;
                        bool.TryParse(claim, out var emailVerified);
                        user.EmailVerified = emailVerified;
                    }

                    httpContext.Items["User"] = user;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to add claims for authenticated user: {ex}");
                }
            }

            await _next(httpContext);
        }
    }
}