using System;
using System.Threading.Tasks;

namespace MTM_Template_Application.Services.Core;

/// <summary>
/// In-memory pub/sub message bus
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// Publish a message
    /// </summary>
    Task PublishAsync<T>(T message) where T : class;

    /// <summary>
    /// Subscribe to messages of a specific type
    /// </summary>
    Task<IDisposable> SubscribeAsync<T>(Func<T, Task> handler) where T : class;

    /// <summary>
    /// Unsubscribe from messages
    /// </summary>
    Task UnsubscribeAsync<T>(IDisposable subscription) where T : class;
}
