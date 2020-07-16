using System.Linq;
using System.Security.Claims;

namespace OpenSpark.ApiGateway.Extensions
{
    public static class ClaimsPrincipleExtensions
    {
        public static string GetFirebaseAuth(this ClaimsPrincipal user)
        {
            if (user.Identity.IsAuthenticated && user.HasClaim(c => c.Type == "user_id"))
            {
                return user.Claims.Single(c => c.Type == "user_id").Value;
            }

            return null;
        }
    }
}