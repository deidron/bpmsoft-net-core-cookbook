namespace EP.EpMessageConsumerExample
{
    /// <summary>
    /// Parameters of <see cref="EpGenerateContactsBackgroundTask"/>. Kept to primitives on purpose:
    /// the runner serializes them with MessagePack before handing them to the executing actor.
    /// </summary>
    public sealed class EpGenerateContactsTaskParameters : IEpGenerateContactsRequest
    {
        /// <summary>
        /// Upper bound for <see cref="Count"/>. It is twenty times the limit of
        /// <see cref="EpGenerateContactCommand"/>, and the difference is the whole point of having
        /// both variants: the message based one is capped by what it does to a queue shared with
        /// the platform's background processes, while this one only occupies one executor of its
        /// own pool and disturbs nobody else.
        /// </summary>
        public const int MaxCount = 1000;

        public int Count { get; set; }
    }
}
