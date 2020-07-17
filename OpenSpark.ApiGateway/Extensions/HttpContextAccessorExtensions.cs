using Microsoft.AspNetCore.Http;
using OpenSpark.Domain;

namespace OpenSpark.ApiGateway.Extensions
{
    public static class HttpContextAccessorExtensions
    {
        public static User GetFirebaseUser(this IHttpContextAccessor contextAccessor)
        {
            // User item is set by FirebaseUserMiddleware
            if (!contextAccessor.HttpContext.Items.ContainsKey("User")) return null;
            return (User)contextAccessor.HttpContext.Items["User"];
        }
    }
}