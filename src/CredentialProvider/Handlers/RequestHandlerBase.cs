using NuGet.Common;
using NuGet.Protocol.Plugins;

namespace NuGet.TokenCredentialProvider;

abstract class RequestHandlerBase<TRequest, TResponse> : IRequestHandler
    where TResponse : class
{
    protected readonly PluginLogger _logger;

    public RequestHandlerBase(PluginLogger logger)
    {
        _logger = logger;
    }

    public CancellationToken CancellationToken { get; }
    public abstract Task<TResponse> HandleRequestAsync(TRequest request, CancellationToken cancellationToken);

    public async Task HandleResponseAsync(IConnection connection, Message message, IResponseHandler responseHandler, CancellationToken cancellationToken)
    {
        _logger.Log(LogLevel.Debug, $"Received request: {_logger.SerializeForLogging(message)}");
        
        var request = MessageUtilities.DeserializePayload<TRequest>(message);
        
        var response = await HandleRequestAsync(request, cancellationToken);

        _logger.Log(LogLevel.Debug, $"Sending response: {_logger.SerializeForLogging(response)}");

        // Allow a little bit of time before sending the response to let log flush.
        // This provides more reliable logging visible in NuGet's logging output.
        await _logger.PauseForEmptyAsync(TimeSpan.FromMilliseconds(500));

        await Task.Delay(1000);

        await responseHandler.SendResponseAsync(message, response, cancellationToken);

        // Only start the logger after we know the log level. If we start sending log messages too early the
        // connection hangs.
        if (message.Method == MessageMethod.SetLogLevel)
        {
            _logger.Log(LogLevel.Debug, "Starting plugin logger.");
            _logger.Start();
        }
    }
}
