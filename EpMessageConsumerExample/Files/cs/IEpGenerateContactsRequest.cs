namespace EP.EpMessageConsumerExample
{
    /// <summary>
    /// A request to generate <see cref="Count"/> contacts, sent either to the message bus or to a
    /// background task. Each implementation keeps its own <c>MaxCount</c>: C# 7.3 has no static
    /// interface members, and the limit must not travel with the serialized request.
    /// </summary>
    public interface IEpGenerateContactsRequest
    {
        int Count { get; set; }
    }
}
