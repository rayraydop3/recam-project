using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RecamNewBackend.Common;

namespace RecamNewBackend.Filters;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument == null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (_serviceProvider.GetService(validatorType) is not IValidator validator)
                continue;

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);

            if (!result.IsValid)
            {
                var errorMessage = string.Join(" ", result.Errors.Select(e => e.ErrorMessage));
                context.Result = new BadRequestObjectResult(ApiResponse<object>.Fail(errorMessage, "VALIDATION_ERROR"));
                return;
            }
        }

        await next();
    }
}
