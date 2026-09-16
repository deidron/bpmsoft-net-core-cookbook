namespace EP.EpMessageConsumerExample
{
    using System.Collections.Generic;
    using BPMSoft.Core;
    using BPMSoft.Core.Entities;

    public interface IEpContactFakeDataGenerator
    {
        IReadOnlyList<Entity> Generate(UserConnection userConnection, int count);
    }
}
