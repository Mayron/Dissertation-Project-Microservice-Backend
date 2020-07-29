using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpenSpark.ApiGateway.Extensions;
using OpenSpark.Shared.ViewModels;

namespace OpenSpark.ApiGateway.Filters
{
    public class FirestoreAuthorizeFilter : IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.GetFirebaseUser();
            if (user != null) return;

            var result = new ValidationResult(false, "Failed to validate user request");
            context.Result = new BadRequestObjectResult(result);
        }
    }

    public class FirestoreAuthorizeAttribute : TypeFilterAttribute
    {
        public FirestoreAuthorizeAttribute() : base(typeof(FirestoreAuthorizeFilter))
        {
        }
    }
}