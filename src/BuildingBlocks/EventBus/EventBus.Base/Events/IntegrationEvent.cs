using Newtonsoft.Json;
using System;

namespace EventBus.Base.Events
{
    /// <summary>
    /// We will send instance of this class to message brokers. RabbitMQ, Azure Service Bus will consume this.
    /// </summary>
    public class IntegrationEvent
    {
        #region Properties
        [JsonProperty]
        public Guid Id { get; private set; }

        [JsonProperty]
        public DateTime CreatedDate { get; private set; } 
        #endregion

        #region ctor
        [JsonConstructor]
        public IntegrationEvent(Guid id, DateTime createdDate)
        {
            Id = id;
            CreatedDate = createdDate;
        }

        public IntegrationEvent()
        {
            Id = Guid.NewGuid();
            CreatedDate = DateTime.Now;
        }
        #endregion
    }
}
