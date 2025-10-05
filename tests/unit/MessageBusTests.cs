using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MTM_Template_Application.Services.Core;
using Xunit;

namespace MTM_Template_Tests.Unit;

/// <summary>
/// Unit tests for MessageBus (T150)
/// Tests pub/sub messaging with concurrent delivery
/// </summary>
public class MessageBusTests
{
    #region PublishAsync Tests

    [Fact]
    public async Task PublishAsync_WithNullMessage_ThrowsArgumentNullException()
    {
        // Arrange
        var messageBus = new MessageBus();

        // Act
        var act = async () => await messageBus.PublishAsync<object>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("message");
    }

    [Fact]
    public async Task PublishAsync_WithNoSubscribers_CompletesSuccessfully()
    {
        // Arrange
        var messageBus = new MessageBus();

        // Act
        var act = async () => await messageBus.PublishAsync(new TestMessage { Content = "test" });

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task PublishAsync_WithOneSubscriber_DeliversMessage()
    {
        // Arrange
        var messageBus = new MessageBus();
        TestMessage? receivedMessage = null;

        await messageBus.SubscribeAsync<TestMessage>(msg =>
        {
            receivedMessage = msg;
            return Task.CompletedTask;
        });

        // Act
        await messageBus.PublishAsync(new TestMessage { Content = "Hello" });

        // Assert
        receivedMessage.Should().NotBeNull();
        receivedMessage!.Content.Should().Be("Hello");
    }

    [Fact]
    public async Task PublishAsync_WithMultipleSubscribers_DeliversToAll()
    {
        // Arrange
        var messageBus = new MessageBus();
        var receivedMessages = new List<string>();

        await messageBus.SubscribeAsync<TestMessage>(msg =>
        {
            receivedMessages.Add($"Subscriber1:{msg.Content}");
            return Task.CompletedTask;
        });

        await messageBus.SubscribeAsync<TestMessage>(msg =>
        {
            receivedMessages.Add($"Subscriber2:{msg.Content}");
            return Task.CompletedTask;
        });

        // Act
        await messageBus.PublishAsync(new TestMessage { Content = "Broadcast" });

        // Assert
        receivedMessages.Should().HaveCount(2);
        receivedMessages.Should().Contain("Subscriber1:Broadcast");
        receivedMessages.Should().Contain("Subscriber2:Broadcast");
    }

    #endregion

    #region SubscribeAsync Tests

    [Fact]
    public async Task SubscribeAsync_WithNullHandler_ThrowsArgumentNullException()
    {
        // Arrange
        var messageBus = new MessageBus();

        // Act
        var act = async () => await messageBus.SubscribeAsync<TestMessage>(null!);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("handler");
    }

    [Fact]
    public async Task SubscribeAsync_ReturnsDisposableSubscription()
    {
        // Arrange
        var messageBus = new MessageBus();

        // Act
        var subscription = await messageBus.SubscribeAsync<TestMessage>(msg => Task.CompletedTask);

        // Assert
        subscription.Should().NotBeNull();
        subscription.Should().BeAssignableTo<IDisposable>();
    }

    #endregion

    #region UnsubscribeAsync Tests

    [Fact]
    public async Task UnsubscribeAsync_AfterDisposingSubscription_StopsReceivingMessages()
    {
        // Arrange
        var messageBus = new MessageBus();
        var messageCount = 0;

        var subscription = await messageBus.SubscribeAsync<TestMessage>(msg =>
        {
            messageCount++;
            return Task.CompletedTask;
        });

        // Act
        subscription.Dispose();
        await messageBus.PublishAsync(new TestMessage { Content = "Should not receive" });

        // Assert
        messageCount.Should().Be(0);
    }

    [Fact]
    public async Task UnsubscribeAsync_DoesNotAffectOtherSubscribers()
    {
        // Arrange
        var messageBus = new MessageBus();
        var subscriber1Count = 0;
        var subscriber2Count = 0;

        var subscription1 = await messageBus.SubscribeAsync<TestMessage>(msg =>
        {
            subscriber1Count++;
            return Task.CompletedTask;
        });

        await messageBus.SubscribeAsync<TestMessage>(msg =>
        {
            subscriber2Count++;
            return Task.CompletedTask;
        });

        // Act
        subscription1.Dispose();
        await messageBus.PublishAsync(new TestMessage());

        // Assert
        subscriber1Count.Should().Be(0);
        subscriber2Count.Should().Be(1);
    }

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public async Task PublishAsync_WithConcurrentPublishers_DeliversAllMessages()
    {
        // Arrange
        var messageBus = new MessageBus();
        var receivedMessages = new System.Collections.Concurrent.ConcurrentBag<int>();

        await messageBus.SubscribeAsync<TestMessage>(msg =>
        {
            if (int.TryParse(msg.Content, out var id))
            {
                receivedMessages.Add(id);
            }
            return Task.CompletedTask;
        });

        // Act - Publish 100 messages concurrently
        var tasks = new List<Task>();
        for (int i = 0; i < 100; i++)
        {
            var messageId = i;
            var task = Task.Factory.StartNew(async () =>
            {
                await messageBus.PublishAsync(new TestMessage { Content = messageId.ToString() });
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
            tasks.Add(task);
        }
        await Task.WhenAll(tasks);

        // Assert
        receivedMessages.Should().HaveCount(100);
        receivedMessages.ToList().Distinct().Should().HaveCount(100);
    }

    [Fact]
    public async Task SubscribeAsync_WithConcurrentSubscriptions_AllReceiveMessages()
    {
        // Arrange
        var messageBus = new MessageBus();
        var subscriptionCount = 50;
        var counters = new int[subscriptionCount];

        // Act - Create 50 concurrent subscriptions
        var subscribeTasks = new List<Task>();
        for (int i = 0; i < subscriptionCount; i++)
        {
            var index = i;
            var task = Task.Factory.StartNew(async () =>
            {
                await messageBus.SubscribeAsync<TestMessage>(msg =>
                {
                    Interlocked.Increment(ref counters[index]);
                    return Task.CompletedTask;
                });
            }, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default).Unwrap();
            subscribeTasks.Add(task);
        }
        await Task.WhenAll(subscribeTasks);

        // Publish a message
        await messageBus.PublishAsync(new TestMessage { Content = "Concurrent test" });

        // Assert - All subscribers should receive the message
        counters.Should().AllSatisfy(count => count.Should().Be(1));
    }

    #endregion

    #region Different Message Types Tests

    [Fact]
    public async Task PublishAsync_WithDifferentMessageTypes_OnlyDeliversToMatchingSubscribers()
    {
        // Arrange
        var messageBus = new MessageBus();
        var testMessageCount = 0;
        var otherMessageCount = 0;

        await messageBus.SubscribeAsync<TestMessage>(msg =>
        {
            testMessageCount++;
            return Task.CompletedTask;
        });

        await messageBus.SubscribeAsync<OtherTestMessage>(msg =>
        {
            otherMessageCount++;
            return Task.CompletedTask;
        });

        // Act
        await messageBus.PublishAsync(new TestMessage { Content = "Test" });
        await messageBus.PublishAsync(new OtherTestMessage { Value = 123 });

        // Assert
        testMessageCount.Should().Be(1);
        otherMessageCount.Should().Be(1);
    }

    #endregion

    #region Helper Classes

    private class TestMessage
    {
        public string Content { get; set; } = string.Empty;
    }

    private class OtherTestMessage
    {
        public int Value { get; set; }
    }

    #endregion
}
