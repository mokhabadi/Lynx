using System;

namespace Lynx
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class HandlerAttribute : Attribute
    {
        public string Name { get; }

        public HandlerAttribute(string name)
        {
            Name = name;
        }
    }
}
