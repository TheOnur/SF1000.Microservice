using EventBus.Base.Events;
using System.Threading.Tasks;

namespace EventBus.Base.Abstraction
{
    public interface IIntegrationEventHandler<TIntegrationEvent> : IntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }

    /// <summary>
    /// It is just for markup
    /// </summary>
    public interface IntegrationEventHandler
    {

    }
}
