namespace EP.EpMassTransitRabbitMqExample.Tests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using MassTransit;
    using NSubstitute;

    internal sealed class EpFakeBus
    {
        public EpFakeBus()
        {
            StartCompletes();
            StopCompletes();
        }

        public IBusControl Bus { get; } = Substitute.For<IBusControl>();

        public BusHandle Handle { get; } = Substitute.For<BusHandle>();

        public CancellationToken StartToken { get; private set; }

        public void StartCompletes() => OnStart(_ => Task.FromResult(Handle));

        public void StartCompletesAfter(TimeSpan delay) => OnStart(async _ =>
        {
            await Task.Delay(delay, CancellationToken.None).ConfigureAwait(false);
            return Handle;
        });

        public void StartCompletesOnCancellation() => OnStart(token =>
        {
            TaskCompletionSource<BusHandle> start = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _ = token.Register(() => start.TrySetException(new InvalidOperationException("Start canceled.")));
            return start.Task;
        });

        public void StartNeverCompletes() => OnStart(_ => new TaskCompletionSource<BusHandle>().Task);

        public void StartFails(Exception exception) => OnStart(_ => Task.FromException<BusHandle>(exception));

        public void StopCompletes() =>
            Handle.StopAsync(Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        public void StopNeverCompletes() =>
            Handle.StopAsync(Arg.Any<CancellationToken>()).Returns(new TaskCompletionSource().Task);

        public void StopFails(Exception exception) =>
            Handle.StopAsync(Arg.Any<CancellationToken>()).Returns(Task.FromException(exception));

        private void OnStart(Func<CancellationToken, Task<BusHandle>> start) =>
            Bus.StartAsync(Arg.Any<CancellationToken>()).Returns(call =>
            {
                CancellationToken token = call.Arg<CancellationToken>();
                StartToken = token;
                return start(token);
            });
    }
}
