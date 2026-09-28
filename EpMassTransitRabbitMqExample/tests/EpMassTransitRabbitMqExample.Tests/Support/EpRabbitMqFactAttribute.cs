namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using Xunit;

    [AttributeUsage(AttributeTargets.Method)]
    public sealed class EpRabbitMqFactAttribute : FactAttribute
    {
        public const string UriVariable = "EP_RABBITMQ_URI";

        public EpRabbitMqFactAttribute()
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(UriVariable)))
                Skip = $"Set {UriVariable} to a RabbitMQ address with a dedicated vhost to run integration tests.";
        }
    }
}
