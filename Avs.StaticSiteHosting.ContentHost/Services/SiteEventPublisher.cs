using MassTransit;
using System.Threading.Channels;

namespace Avs.StaticSiteHosting.ContentHost.Services;

public interface ISiteEventPublisher : IHostedService
{
    /// <summary>
    /// Adds an event message to channel for publishing in background
    /// </summary>
    /// <param name="evt">Event</param>
    void PublishEvent(object @evt);
}

public class SiteEventPublisher(IServiceProvider serviceProvider) : ISiteEventPublisher
{
    private readonly Channel<object> _eventChannel = Channel.CreateUnbounded<object>();
    private Task? _runTask;

    public void PublishEvent(object @evt)
    {
        _eventChannel.Writer.TryWrite(@evt);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _runTask = RunAsync(cancellationToken);
        if (_runTask.IsCompleted)
        {
            return _runTask;
        }

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {        
        return Task.CompletedTask;
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceProvider.CreateScope();
        var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var message = await _eventChannel.Reader.ReadAsync(cancellationToken);
                if (message is null)
                {
                    continue;
                }

                await publishEndpoint.Publish(message, cancellationToken);
            }
            catch (Exception)
            {
                // no-op
            }
        }
    }
}