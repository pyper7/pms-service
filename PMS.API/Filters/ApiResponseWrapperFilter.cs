using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PMS.Application.Models;

namespace PMS.API.Filters;

public class ApiResponseWrapperFilter : IAsyncResultFilter
{
    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        if (context.Result is ObjectResult objectResult)
        {
            if (objectResult.Value is not ApiResponse<object> &&
                !(objectResult.Value?.GetType().IsGenericType == true && objectResult.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>)))
            {
                var wrapped = Activator.CreateInstance(typeof(ApiResponse<>).MakeGenericType(objectResult.Value?.GetType() ?? typeof(object)))!;
                wrapped.GetType().GetProperty("Success")!.SetValue(wrapped, true);
                wrapped.GetType().GetProperty("Message")!.SetValue(wrapped, "Success");
                wrapped.GetType().GetProperty("Data")!.SetValue(wrapped, objectResult.Value);
                context.Result = new ObjectResult(wrapped)
                {
                    StatusCode = objectResult.StatusCode
                };
            }
        }

        await next();
    }
}


