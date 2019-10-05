using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace System.Composition.Extensions.DependencyInjection
{
    /// <summary>
    /// An implementation of IServiceScopeFactory that uses MEF to resolve dependencies, with an optional fallback IServiceProvider 
    /// for resolving dependcies not registered with MEF
    /// </summary>
    public sealed class MefServiceProvider : MefServiceScope, IServiceScopeFactory
    {
        private readonly ExportFactory<CompositionContext> _requestScopeFactory;
        private IServiceProvider? _fallback;

        /// <inheritdoc/>
        protected override Type MefServiceFallbackType => typeof(MefServiceRootFallback);

        /// <inheritdoc/>
        protected override IServiceProvider? Fallback => _fallback;

        /// <summary>
        /// Creates a new MefServiceProvider that uses the given MEF container to resolve dependencies
        /// </summary>
        public MefServiceProvider(CompositionHost container)
            : base(new Export<CompositionContext>(container, container.Dispose), null, null)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var factoryContract = new CompositionContract(typeof(ExportFactory<CompositionContext>), null, new Dictionary<string, object> {
                { "SharingBoundaryNames", new[] { nameof(ServiceLifetime.Scoped) } }
            });
            _requestScopeFactory = (ExportFactory<CompositionContext>)container.GetExport(factoryContract);
        }

        /// <summary>
        /// Sets the ServiceProvider to use as a fallback for dependencies that can't be resolved by MEF
        /// </summary>
        public void SetFallback(IServiceProvider fallback) => _fallback = fallback;

        /// <inheritdoc/>
        public IServiceScope CreateScope() => new MefServiceScope(_requestScopeFactory.CreateExport(), this, _fallback?.GetService<IServiceScopeFactory>().CreateScope());
    }

    /// <summary>
    /// An implementation of IServiceScope that uses MEF to resolve dependencies, with an optional fallback IServiceProvider 
    /// for resolving dependcies not registered with MEF
    /// </summary>
    public class MefServiceScope : IServiceProvider, IServiceScope
    {
        private readonly IServiceScopeFactory? _parentFactory;
        private readonly IServiceScope? _fallback;
        private readonly Export<CompositionContext> _compositionScope;
        private IMefServiceFallback? _serviceScopeFallback;

        /// <summary>
        /// The ServiceProvider being used as a fallback for dependencies that can't be resolved by MEF
        /// </summary>
        protected virtual IServiceProvider? Fallback => _fallback?.ServiceProvider;

        protected virtual Type MefServiceFallbackType => typeof(MefServiceScopeFallback);

        private CompositionContext CompositionScope => _compositionScope.Value;

        /// <inheritdoc/>
        public IServiceProvider ServiceProvider => this;

        /// <summary>
        /// Creates a new service scope from the given MEF sub-container and fallback service scope
        /// </summary>
        public MefServiceScope(Export<CompositionContext> compositionScope, IServiceScopeFactory? parent, IServiceScope? fallback)
        {
            _compositionScope = compositionScope;
            _parentFactory = parent;
            _fallback = fallback;
        }

        /// <inheritdoc/>
        public object? GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            if (_serviceScopeFallback == null)
            {
                _serviceScopeFallback = SetupScopeFallback();
            }
            if (serviceType == typeof(IServiceProvider) || serviceType == typeof(MefServiceScope))
            {
                return this;
            }
            if (serviceType == typeof(IServiceScopeFactory))
            {
                if (_parentFactory != null)
                {
                    return _parentFactory;
                }
                if (this is IServiceScopeFactory factory)
                {
                    return factory;
                }
            }
            if (CompositionScope.TryGetExport(serviceType, null, out var result))
            {
                return result;
            }
            return Fallback?.GetService(serviceType);
        }

        protected virtual IMefServiceFallback? SetupScopeFallback()
        {
            if (Fallback != null
                && CompositionScope.TryGetExport(MefServiceFallbackType, null, out var serviceScopeFallbackExport)
                && serviceScopeFallbackExport is IMefServiceFallback serviceScopeFallback)
            {
                serviceScopeFallback.FallbackServiceProvider = Fallback;
                return serviceScopeFallback;
            }
            return _serviceScopeFallback;
        }

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _compositionScope.Dispose();
                    _fallback?.Dispose();
                }
                disposedValue = true;
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
        }
    }
}