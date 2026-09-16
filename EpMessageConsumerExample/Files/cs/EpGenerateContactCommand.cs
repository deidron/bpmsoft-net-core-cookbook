namespace EP.EpMessageConsumerExample
{
    public sealed class EpGenerateContactCommand
    {
        /// <summary>
        /// Upper bound for <see cref="Count"/>, deliberately modest. This command fans out into one
        /// message per contact, the application has a single incoming queue shared with the
        /// platform's background processes, and every message costs a fresh UserConnection with a
        /// full login. The limit belongs here rather than to the generator: producing fake data is
        /// cheap, flooding the queue is not.
        /// </summary>
        public const int MaxCount = 50;

        public int Count { get; set; }
    }
}
