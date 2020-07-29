using Microsoft.AspNetCore.Http;
using OpenSpark.Domain;

namespace OpenSpark.ApiGateway.Extensions
{
    public static class HttpContextAccessorExtensions
    {
        public static User GetFirebaseUser(this IHttpContextAccessor contextAccessor)
        {
            return contextAccessor.HttpContext.GetFirebaseUser();
        }

        public static User GetFirebaseUser(this HttpContext context)
        {
            // User item is set by FirebaseUserMiddleware
            if (!context.Items.ContainsKey("User")) return null;
            return (User)context.Items["User"];
        }
    }
}