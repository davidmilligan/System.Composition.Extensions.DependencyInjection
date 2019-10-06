using System;
using System.Composition;
using System.Composition.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace System.Composition.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for setting up the host to use MEF (System.Composition) to resolve dependencies
    /// </summary>
    public static class HostingExtension
    {
        private static MefServiceProviderFactory? _factory;

        /// <summary>
        /// Sets up the host to use MEF (System.Composition) to resolve dependencies
        /// </summary>
        public static IWebHostBuilder UseMef(this IWebHostBuilder hostBuilder)
        {
            _factory = new MefServiceProviderFactory();
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<ContainerConfiguration>>(_factory));
            });
        }

        /// <summary>
        /// Sets up the host to use MEF (System.Composition) to resolve dependencies
        /// </summary>
        public static IHostBuilder UseMef(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseServiceProviderFactory(new MefServiceProviderFactory());
        }
    }
}
