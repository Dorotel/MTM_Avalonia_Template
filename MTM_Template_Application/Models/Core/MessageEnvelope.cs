using System;

namespace MTM_Template_Application.Models.Core;

/// <summary>
/// Message envelope for pub/sub messaging
/// </summary>
public class MessageEnvelope
{
    /// <summary>
    /// Unique message identifier
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Message type/topic
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Message payload
    /// </summary>
    public object? Payload { get; set; }

    /// <summary>
    /// When the message was created
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// Correlation ID for tracking related messages
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Number of delivery attempts
    /// </summary>
    public int DeliveryCount { get; set; }

    /// <summary>
    /// Optional expiration time for the message
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
}
