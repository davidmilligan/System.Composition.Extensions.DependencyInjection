using System;
using System.Composition;
using Microsoft.Extensions.DependencyInjection;

namespace System.Composition.Extensions.DependencyInjection
{
    /// <summary>
    /// Attribute that indicates the export should be shared within the scope of a single request
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ScopedAttribute : SharedAttribute
    {
        /// <summary>
        /// Attribute that indicates the export should be shared within the scope of a single request
        /// </summary>
        public ScopedAttribute() : base(nameof(ServiceLifetime.Scoped)) { }
    }
}