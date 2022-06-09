using EventBus.Base.Events;
using System;

namespace EventBus.Base.Abstraction
{
    /// <summary>
    /// Be able to use different types of message brokers.
    /// In this project, I will apply both Azure Service Bus and RabbitMQ.
    /// </summary>
    public interface IEventBus : IDisposable
    {
        void Publish(IntegrationEvent @event);
        void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
        void UnSubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
    }
}
