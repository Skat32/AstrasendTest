using Astrasend.Infrastructure.Np.Policy.ClientPolicy.Interfaces;
using Astrasend.Infrastructure.Np.Policy.Settings;
using Microsoft.Extensions.Options;
using Polly;
using Serilog;

namespace Astrasend.Infrastructure.Np.Policy.ClientPolicy;

/// <inheritdoc />
public class ClientPolicy : IClientPolicy
{
    private readonly OutRequestRetrySettings _retrySettings;

    /// <summary> ctor </summary>
    public ClientPolicy(IOptions<OutRequestRetrySettings> options)
    {
        _retrySettings = options.Value;
    }
    
    /// <inheritdoc />
    public IAsyncPolicy<HttpResponseMessage> GetPolicy(PolicyBuilder<HttpResponseMessage> policy)
        => ConfigurePolicy(policy);

    protected virtual IAsyncPolicy<HttpResponseMessage> ConfigurePolicy(PolicyBuilder<HttpResponseMessage> policy) =>
        policy.OrResult(x => !x.IsSuccessStatusCode)
            .WaitAndRetryAsync(_retrySettings.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)))
            .WrapAsync(Polly.Policy.TimeoutAsync(_retrySettings.TimeoutInSeconds, (_, _, _) =>
            {
                Log.Logger.Information("HTTP OutRequest has been cancelled");
                return Task.CompletedTask;
            }));
    

    /// <inheritdoc />
    public async Task<HttpResponseMessage> UsePolicy(Func<Task<HttpResponseMessage>> func)
        => await GetPolicy().ExecuteAsync(func);
    
    protected virtual IAsyncPolicy<HttpResponseMessage> GetPolicy()
        => ConfigurePolicy(Polly.Policy.HandleResult<HttpResponseMessage>(x => !x.IsSuccessStatusCode));
}
