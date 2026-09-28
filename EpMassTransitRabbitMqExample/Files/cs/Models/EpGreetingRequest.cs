namespace EP.EpMassTransitRabbitMqExample
{
    using System.ComponentModel.DataAnnotations;

    public class EpGreetingRequest
    {
        [Required]
        public string Name { get; set; }
    }
}
