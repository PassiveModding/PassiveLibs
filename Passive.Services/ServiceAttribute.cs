using System;

namespace Passive
{
    // Interface used for reflection to auto-generate the service provider.
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ServiceAttribute : Attribute
    {
    }
}