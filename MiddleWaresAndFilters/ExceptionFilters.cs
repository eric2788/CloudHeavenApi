using System.Linq;
using CloudHeavenApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CloudHeavenApi.MiddleWaresAndFilters
{
    public class ExceptionFilters : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            context.HttpContext.Response.ContentType = "application/json";
            var exception = context.Exception;
            if (exception is AuthException authE)
            {
                var response = authE.ErrorResponse;
                context.Result = new UnauthorizedObjectResult(response);
            }
            else
            {
                var response = new ErrorResponse
                {
                    Error = exception.GetType().Name,
                    ErrorMessage = exception.Message,
                    Cause = exception.StackTrace.Split("\r\n").Select(s => s.Trim()).ToArray(),
                    CauseFrom = exception.Source
                };
                context.Result = new BadRequestObjectResult(response);
            }
        }
    }
}