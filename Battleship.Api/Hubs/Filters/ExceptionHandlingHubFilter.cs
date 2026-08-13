using FluentValidation;
using Microsoft.AspNetCore.SignalR;
using Battleship.Api.Exceptions;

namespace Battleship.Api.Hubs.Filters;

public class ExceptionHandlingHubFilter(ILogger<ExceptionHandlingHubFilter> logger) : IHubFilter
{
    private readonly ILogger<ExceptionHandlingHubFilter> _logger = logger;
    
    public async ValueTask<object?> InvokeMethodAsync(HubInvocationContext context, Func<HubInvocationContext, ValueTask<object?>> next)
    {
        try
        {
            return await next(context);
        }
        catch (ValidationException ex)
        {
            string message = string.Join("; ", ex.Errors
                .Select(e => e.ErrorMessage));
            
            throw new HubException(message);
            
        }
        catch (BattleshipException ex)
        {
            throw new HubException(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception in hub method {Method}", context.HubMethod.Name);
            throw; 
        }
    }
}