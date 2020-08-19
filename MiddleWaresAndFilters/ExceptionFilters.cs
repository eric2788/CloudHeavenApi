using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudHeavenApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

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