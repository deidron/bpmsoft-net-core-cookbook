namespace EP.EpAutofacContainerExample
{
    internal sealed class EpGreetingService(IEpClock clock) : IEpGreetingService
    {
        public string Greet(string name) => $"{GetPartOfDay()}, {name}!";

        private string GetPartOfDay() =>
            clock.Now.Hour switch
            {
                < 6 => "Good night",
                < 12 => "Good morning",
                < 18 => "Good afternoon",
                _ => "Good evening"
            };
    }
}
