using System;

namespace GrowAGarden.Core._01.Scripts.Core.Dependencies
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class InjectAttribute : Attribute
    {
        public bool Optional { get; set; } = false;
        
        public InjectAttribute(bool optional = false)
        {
            Optional = optional;
        }
    }
}