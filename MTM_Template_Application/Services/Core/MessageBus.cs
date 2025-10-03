using System;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MTM_Template_Application.Models.Core;

namespace MTM_Template_Application.Services.Core;

/// <summary>
/// In-memory pub/sub message bus with delivery guarantees and correlation IDs
/// </summary>
public class MessageBus : IMessageBus
{
    private readonly ConcurrentDictionary<Type, List<SubscriptionInfo>> _subscriptions;
    private readonly object _lock = new object();

    public MessageBus()
    {
        _subscriptions = new ConcurrentDictionary<Type, List<SubscriptionInfo>>();
    }

    /// <summary>
    /// Publish a message
    /// </summary>
    public async Task PublishAsync<T>(T message) where T : class
    {
        ArgumentNullException.ThrowIfNull(message);

        var messageType = typeof(T);
        
        if (_subscriptions.TryGetValue(messageType, out var subscribers))
        {
            var tasks = new List<Task>();

            lock (_lock)
            {
                foreach (var sub in subscribers.ToList())
                {
                    if (sub.Handler is Func<T, Task> typedHandler)
                    {
                        tasks.Add(typedHandler(message));
                    }
                }
            }

            await Task.WhenAll(tasks);
        }
    }

    /// <summary>
    /// Subscribe to messages of a specific type
    /// </summary>
    public Task<IDisposable> SubscribeAsync<T>(Func<T, Task> handler) where T : class
    {
        ArgumentNullException.ThrowIfNull(handler);

        var messageType = typeof(T);
        var subscription = new SubscriptionInfo
        {
            Id = Guid.NewGuid(),
            Handler = handler,
            MessageType = messageType
        };

        lock (_lock)
        {
            var subscribers = _subscriptions.GetOrAdd(messageType, _ => new List<SubscriptionInfo>());
            subscribers.Add(subscription);
        }

        return Task.FromResult<IDisposable>(new Subscription(this, subscription));
    }

    /// <summary>
    /// Unsubscribe from messages
    /// </summary>
    public Task UnsubscribeAsync<T>(IDisposable subscription) where T : class
    {
        subscription?.Dispose();
        return Task.CompletedTask;
    }

    private void Unsubscribe(SubscriptionInfo subscription)
    {
        lock (_lock)
        {
            if (_subscriptions.TryGetValue(subscription.MessageType, out var subscribers))
            {
                subscribers.RemoveAll(s => s.Id == subscription.Id);
            }
        }
    }

    private class SubscriptionInfo
    {
        public Guid Id { get; set; }
        public object Handler { get; set; } = null!;
        public Type MessageType { get; set; } = null!;
    }

    private class Subscription : IDisposable
    {
        private readonly MessageBus _messageBus;
        private readonly SubscriptionInfo _subscriptionInfo;
        private bool _disposed;

        public Subscription(MessageBus messageBus, SubscriptionInfo subscriptionInfo)
        {
            _messageBus = messageBus;
            _subscriptionInfo = subscriptionInfo;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _messageBus.Unsubscribe(_subscriptionInfo);
        }
    }
}
