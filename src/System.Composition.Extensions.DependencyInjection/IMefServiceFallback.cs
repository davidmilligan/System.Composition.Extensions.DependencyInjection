using System;
using System.Composition;

namespace System.Composition.Extensions.DependencyInjection
{
    /// <summary>
    /// Interface for an export type that works in concert with DependencyInjectionProvider to allow [Import]ing services from the Asp.Net Core DI container via MEF
    /// </summary>
    public interface IMefServiceFallback
    {
        IServiceProvider FallbackServiceProvider { get; set; }
    }

    [Export, Scoped]
    public class MefServiceScopeFallback : IMefServiceFallback
    {
        public IServiceProvider FallbackServiceProvider { get; set; }
    }

    [Export, Shared]
    public class MefServiceRootFallback : IMefServiceFallback
    {
        public IServiceProvider FallbackServiceProvider { get; set; }
    }
}