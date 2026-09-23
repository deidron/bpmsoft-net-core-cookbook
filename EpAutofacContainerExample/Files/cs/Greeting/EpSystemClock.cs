namespace EP.EpAutofacContainerExample
{
    using System;

    internal sealed class EpSystemClock : IEpClock
    {
        public DateTime Now => DateTime.Now;
    }
}
