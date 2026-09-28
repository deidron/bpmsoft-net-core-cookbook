namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using Autofac;

    internal sealed class EpDisposeHook(Action onDispose) : IDisposable
    {
        private readonly Action _onDispose = onDispose;

        public void Dispose() => _onDispose();

        public static IContainer BuildContainer(Action onDispose)
        {
            ContainerBuilder builder = new();
            _ = builder.Register(_ => new EpDisposeHook(onDispose)).SingleInstance().AutoActivate();
            return builder.Build();
        }
    }
}
