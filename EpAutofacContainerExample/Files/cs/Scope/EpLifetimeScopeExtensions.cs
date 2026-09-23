namespace EP.EpAutofacContainerExample
{
    using System;
    using Autofac;
    using Autofac.Builder;
    using BPMSoft.Core;

    internal static class EpLifetimeScopeExtensions
    {
        internal const string UserConnectionScopeTag = "EpUserConnectionScope";

        internal static ILifetimeScope BeginUserConnectionScope(this ILifetimeScope scope,
            UserConnection userConnection, Action<ContainerBuilder> configure = null)
        {
            ArgumentNullException.ThrowIfNull(scope);
            ArgumentNullException.ThrowIfNull(userConnection);
            return scope.BeginLifetimeScope(UserConnectionScopeTag, builder =>
            {
                _ = builder.RegisterInstance(userConnection).ExternallyOwned();
                configure?.Invoke(builder);
            });
        }

        internal static IRegistrationBuilder<TLimit, TActivatorData, TStyle>
            InstancePerUserConnection<TLimit, TActivatorData, TStyle>(
                this IRegistrationBuilder<TLimit, TActivatorData, TStyle> registration)
        {
            ArgumentNullException.ThrowIfNull(registration);
            return registration.InstancePerMatchingLifetimeScope(UserConnectionScopeTag);
        }
    }
}
