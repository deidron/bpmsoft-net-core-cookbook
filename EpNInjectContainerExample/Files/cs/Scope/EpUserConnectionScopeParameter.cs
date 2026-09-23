namespace EP.EpNInjectContainerExample
{
    using Ninject.Parameters;

    internal sealed class EpUserConnectionScopeParameter(EpUserConnectionScope scope)
        : Parameter(nameof(EpUserConnectionScope), scope, shouldInherit: true)
    {
        internal EpUserConnectionScope Scope => scope;
    }
}
