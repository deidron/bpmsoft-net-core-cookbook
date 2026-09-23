namespace EP.EpNInjectContainerExample
{
    using System;
    using BPMSoft.Core;
    using Ninject;
    using Ninject.Infrastructure.Disposal;
    using Ninject.Syntax;

    internal sealed class EpUserConnectionScope : INotifyWhenDisposed
    {
        private readonly IResolutionRoot _root;

        internal EpUserConnectionScope(IResolutionRoot root, UserConnection userConnection)
        {
            ArgumentNullException.ThrowIfNull(root);
            ArgumentNullException.ThrowIfNull(userConnection);
            _root = root;
            UserConnection = userConnection;
        }

        public event EventHandler Disposed;

        public bool IsDisposed { get; private set; }

        internal UserConnection UserConnection { get; }

        internal T Get<T>()
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            return _root.Get<T>(new EpUserConnectionScopeParameter(this));
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            Disposed?.Invoke(this, EventArgs.Empty);
            Disposed = null;
        }
    }
}
