namespace EP.EpMassTransitRabbitMqExample
{
    using System.Threading.Tasks;

    internal interface IEpGreetingPublisher
    {
        Task<EpGreetingPublishResult> PublishAsync(string name);
    }
}
