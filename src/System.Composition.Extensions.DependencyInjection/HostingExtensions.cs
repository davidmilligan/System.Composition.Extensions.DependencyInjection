using System;
using System.Composition;
using System.Composition.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace System.Composition.Extensions.DependencyInjection
{
    public static class HostingExtension
    {
        private static MefServiceProviderFactory _factory;

        public static IWebHostBuilder UseMef(this IWebHostBuilder hostBuilder)
        {
            _factory = new MefServiceProviderFactory();
            return hostBuilder.ConfigureServices((context, services) =>
            {
                services.Replace(ServiceDescriptor.Singleton<IServiceProviderFactory<ContainerConfiguration>>(_factory));
            });
        }
    }
}
