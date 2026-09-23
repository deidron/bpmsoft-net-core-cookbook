namespace EP.EpNInjectContainerExample
{
    using BPMSoft.Core;

    public interface IEpNotifierFactory
    {
        IEpNotifier Create(UserConnection userConnection);
    }
}
