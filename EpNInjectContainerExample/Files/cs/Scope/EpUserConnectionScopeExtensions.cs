namespace EP.EpNInjectContainerExample
{
    using System;
    using System.Linq;
    using BPMSoft.Core;
    using Ninject.Activation;
    using Ninject.Syntax;

    internal static class EpUserConnectionScopeExtensions
    {
        internal static EpUserConnectionScope BeginUserConnectionScope(this IResolutionRoot root,
            UserConnection userConnection) => new(root, userConnection);

        internal static IBindingNamedWithOrOnSyntax<T> InUserConnectionScope<T>(this IBindingInSyntax<T> binding)
        {
            ArgumentNullException.ThrowIfNull(binding);
            return binding.InScope(context => context.GetUserConnectionScope());
        }

        internal static EpUserConnectionScope GetUserConnectionScope(this IContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return context.Parameters.OfType<EpUserConnectionScopeParameter>().FirstOrDefault()?.Scope
                ?? throw new InvalidOperationException(
                    $"{context.Request.Service.Name} can be resolved only inside a user connection scope.");
        }
    }
}
