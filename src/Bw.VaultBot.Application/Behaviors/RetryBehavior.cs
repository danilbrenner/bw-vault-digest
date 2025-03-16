using Bw.VaultBot.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bw.VaultBot.Application.Behaviors;

public class RetryBehavior<TRequest, TResponse>(ILogger<RetryBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    private async Task<TResponse> Retry(RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken,
        int retryCount = 5,
        int delayInMilliseconds = 100,
        List<Exception>? exceptions = default)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex,
                "An error occurred while handling the request {TRequest}, retry count {Count}", typeof(TRequest),
                retryCount);
            
            if (retryCount < 1 || cancellationToken.IsCancellationRequested)
                throw new AggregateException((exceptions ?? []).FAdd(ex));
            
            await Task.Delay(delayInMilliseconds, cancellationToken);
            
            return await Retry(next, cancellationToken, retryCount - 1, 
                delayInMilliseconds * 2, (exceptions ?? []).FAdd(ex));
        }
    }

    public async Task<TResponse> Handle(TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        return await Retry(next, cancellationToken);
    }
}