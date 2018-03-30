using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SecuroteckWebApplicationCore.Controllers.Authorisation
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
    public class CustomAuthoriseAttribute : ActionFilterAttribute
    {
      /*  public override void OnActionExecuting(HttpActionContext actionContext)
        {
            if(!Thread.CurrentPrincipal.Identity.IsAuthenticated) // If the principle.identity on the current thread is not authenticated
            {
                // Respond with an 'Unauthorised' status code and error
                actionContext.Response = actionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Unauthorized. Check ApiKey in Header is correct.");
            }
        }*/

    }
}