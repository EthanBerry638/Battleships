using System.Reflection;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.SignalR;

namespace Battleship.Api.Hubs.Filters;

public class ValidationHubFilter(IServiceProvider serviceProvider) : IHubFilter
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private static readonly NullabilityInfoContext NullabilityContext = new();

    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext context, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        ParameterInfo[] parameters = context.HubMethod.GetParameters();

        for (int i = 0; i < context.HubMethodArguments.Count; i++)
        {
            object? argument = context.HubMethodArguments[i];
            ParameterInfo parameter = parameters[i];

            if (argument is null)
            {
                bool isNullable = NullabilityContext.Create(parameter).WriteState is NullabilityState.Nullable;
                if (!isNullable)
                    throw new ValidationException($"{parameter.Name} is required and cannot be null.");

                continue;
            }

            Type validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (_serviceProvider.GetService(validatorType) is not IValidator validator) continue;
            
            var validationContext = new ValidationContext<object>(argument);
            ValidationResult result = await validator.ValidateAsync(validationContext);
            if (!result.IsValid)
                throw new ValidationException(result.Errors);
        }

        return await next(context);
    }
}