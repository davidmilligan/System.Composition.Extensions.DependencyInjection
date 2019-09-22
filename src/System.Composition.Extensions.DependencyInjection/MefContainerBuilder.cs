using System;
using System.Composition;
using System.Composition.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace System.Composition.Extensions.DependencyInjection
{
    public class MefContainerBuilder
    {
        public ContainerConfiguration ContainerConfiguration { get; } = new ContainerConfiguration();
        public IServiceCollection ServiceCollection { get; set; }
    }
}