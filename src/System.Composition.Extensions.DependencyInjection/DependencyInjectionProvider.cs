using System;
using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.Composition.Hosting.Core;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace System.Composition.Extensions.DependencyInjection
{
    /// <summary>
    /// ExportDescriptorProvider implementation that allows [Import]ing services from the underlying Asp.Net Core DI container via MEF
    /// </summary>
    public class DependencyInjectionProvider : ExportDescriptorProvider
    {
        public override IEnumerable<ExportDescriptorPromise> GetExportDescriptors(CompositionContract contract, DependencyAccessor descriptorAccessor)
        {
            var implementations = descriptorAccessor.ResolveDependencies("test", contract, false);
            if (!implementations.Any())
            {
                var promise = new ExportDescriptorPromise(
                    contract,
                    contract.ContractType.Name,
                    false,
                    NoDependencies,
                    _ => ExportDescriptor.Create((c, o) =>
                    {
                        object export;
                        if ((c.ToString() != "Root Lifetime Context" && c.TryGetExport(typeof(MefServiceScopeFallback), out export)
                            || c.TryGetExport(typeof(MefServiceRootFallback), out export))
                            && export is IMefServiceFallback serviceProvider)
                        {
                            return serviceProvider.FallbackServiceProvider?.GetService(contract.ContractType);
                        }
                        return null;
                    }, NoMetadata));
                return new[] { promise };
            }
            return NoExportDescriptors;
        }
    }
}