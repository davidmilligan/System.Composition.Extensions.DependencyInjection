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
    public class MefServiceProviderFactory : IServiceProviderFactory<ContainerConfiguration>
    {
        private IServiceCollection _services;

        public ContainerConfiguration CreateBuilder(IServiceCollection services)
        {
            _services = services;
            return new ContainerConfiguration();
        }

        public IServiceProvider CreateServiceProvider(ContainerConfiguration containerBuilder)
        {
            if (containerBuilder == null) throw new ArgumentNullException(nameof(containerBuilder));
            var provider = new MefServiceProvider(containerBuilder.CreateContainer());
            _services.Replace(ServiceDescriptor.Singleton<IServiceScopeFactory>(provider));
            _services.Replace(ServiceDescriptor.Singleton<IServiceProvider>(provider));

            var defaultContextFactoryType = _services.First(t => t.ServiceType == typeof(IHttpContextFactory)).ImplementationType;
            if (defaultContextFactoryType.GetConstructor(new[] { typeof(IServiceProvider) }) != null)
            {
                var mefHttpContextFactory = new MefHttpContextFactory();
                _services.AddSingleton<IHttpContextFactory>(mefHttpContextFactory);
                mefHttpContextFactory.Initialize(defaultContextFactoryType, provider);
            }
            provider.SetFallback(_services.BuildServiceProvider());
            return provider;
        }
    }

    // fix for bug in aspnetcore that results in our custom IServiceProvider not actually being used for requests
    internal class MefHttpContextFactory : IHttpContextFactory
    {
        private Lazy<IHttpContextFactory> _defaultFactory;

        public void Initialize(Type defaultFactoryType, IServiceProvider serviceProvider)
        {
            _defaultFactory = new Lazy<IHttpContextFactory>(() => (IHttpContextFactory)Activator.CreateInstance(defaultFactoryType, serviceProvider));
        }

        public HttpContext Create(IFeatureCollection featureCollection) => _defaultFactory.Value.Create(featureCollection);

        public void Dispose(HttpContext httpContext) => _defaultFactory.Value.Dispose(httpContext);
    }
}
