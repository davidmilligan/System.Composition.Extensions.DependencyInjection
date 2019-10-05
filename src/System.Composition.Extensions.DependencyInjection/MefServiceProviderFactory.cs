using System;
using System.Composition;
using System.Composition.Convention;
using System.Composition.Extensions;
using System.Composition.Hosting;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace System.Composition.Extensions.DependencyInjection
{
    /// <summary>
    /// An implementation of IServiceProviderFactory that integrates MEF (System.Composition) into Asp.Net Core's DI framework, 
    /// and allows configuring the MEF container in the Startup class
    /// </summary>
    public class MefServiceProviderFactory : IServiceProviderFactory<ContainerConfiguration>
    {
        private IServiceCollection? _services;

        /// <inheritdoc/>
        public ContainerConfiguration CreateBuilder(IServiceCollection services)
        {
            _services = services;
            return new ContainerConfiguration();
        }

        /// <inheritdoc/>
        public IServiceProvider CreateServiceProvider(ContainerConfiguration containerBuilder)
        {
            if (containerBuilder == null) { throw new ArgumentNullException(nameof(containerBuilder)); }
            if (_services == null) { throw new InvalidOperationException(); }
            containerBuilder.WithProvider(new DependencyInjectionProvider());
            containerBuilder.WithAssembly(typeof(MefServiceScopeFallback).Assembly);
            var provider = new MefServiceProvider(containerBuilder.CreateContainer());
            _services.Replace(ServiceDescriptor.Singleton<IServiceScopeFactory>(provider));
            _services.Replace(ServiceDescriptor.Singleton<IServiceProvider>(provider));

            var defaultContextFactoryType = _services.First(t => t.ServiceType == typeof(IHttpContextFactory)).ImplementationType;
            if (defaultContextFactoryType.GetConstructor(new[] { typeof(IServiceProvider) }) != null)
            {
                _services.AddSingleton<IHttpContextFactory>(new MefHttpContextFactory(defaultContextFactoryType, provider));
            }
            provider.SetFallback(_services.BuildServiceProvider());
            return provider;
        }
    }

    // fix for bug in aspnetcore that results in our custom IServiceProvider not actually being used for requests
    internal class MefHttpContextFactory : IHttpContextFactory
    {
        private readonly Lazy<IHttpContextFactory> _defaultFactory;

        public MefHttpContextFactory(Type defaultFactoryType, IServiceProvider serviceProvider)
        {
            _defaultFactory = new Lazy<IHttpContextFactory>(() => (IHttpContextFactory)Activator.CreateInstance(defaultFactoryType, serviceProvider));
        }

        public HttpContext Create(IFeatureCollection featureCollection) => _defaultFactory.Value.Create(featureCollection);

        public void Dispose(HttpContext httpContext) => _defaultFactory.Value.Dispose(httpContext);
    }
}
