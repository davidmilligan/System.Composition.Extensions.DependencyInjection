using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace System.Composition.Extensions.DependencyInjection
{
    public sealed class MefServiceProvider : MefServiceScope, IServiceScopeFactory
    {
        private readonly ExportFactory<CompositionContext> _requestScopeFactory;
        private IServiceProvider _fallback;

        protected override IServiceProvider Fallback => _fallback;

        public MefServiceProvider(CompositionHost container)
            : base(new Export<CompositionContext>(container, container.Dispose), null, null)
        {
            if (container == null) throw new ArgumentNullException(nameof(container));
            var factoryContract = new CompositionContract(typeof(ExportFactory<CompositionContext>), null, new Dictionary<string, object> {
                { "SharingBoundaryNames", new[] { nameof(ServiceLifetime.Scoped) } }
            });
            _requestScopeFactory = (ExportFactory<CompositionContext>)container.GetExport(factoryContract);
        }

        public void SetFallback(IServiceProvider fallback) => _fallback = fallback;

        public IServiceScope CreateScope() => new MefServiceScope(_requestScopeFactory.CreateExport(), this, _fallback?.GetService<IServiceScopeFactory>().CreateScope());
    }

    public class MefServiceScope : IServiceProvider, IServiceScope
    {
        private readonly IServiceScopeFactory _parentFactory;
        private readonly IServiceScope _fallback;
        protected virtual IServiceProvider Fallback => _fallback.ServiceProvider;

        protected CompositionContext CompositionScope => _compositionScope.Value;

        public IServiceProvider ServiceProvider => this;

        private readonly Export<CompositionContext> _compositionScope;

        public MefServiceScope(Export<CompositionContext> compositionScope, IServiceScopeFactory parent, IServiceScope fallback)
        {
            Trace.WriteLine("Scope Created");
            _compositionScope = compositionScope;
            _parentFactory = parent;
            _fallback = fallback;
        }

        public object GetService(Type serviceType)
        {
            if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));
            Trace.WriteLine($"GetService {serviceType.Name}");
            if (serviceType == typeof(IServiceProvider))
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

        private bool disposedValue = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Trace.WriteLine("Scope Disposed");
                    _compositionScope.Dispose();
                    _fallback?.Dispose();
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}