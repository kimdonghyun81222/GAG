using System;

namespace GrowAGarden.Core._01.Scripts.Core.Dependencies
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class ProvideAttribute : Attribute
    {
        public Type ServiceType { get; set; }
        public bool Singleton { get; set; } = true;
        
        public ProvideAttribute(Type serviceType = null, bool singleton = true)
        {
            ServiceType = serviceType;
            Singleton = singleton;
        }
    }
}