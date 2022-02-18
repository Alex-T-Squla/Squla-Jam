using System;

namespace Squla.Core.IOC
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public class InjectComponent : Attribute
    {
        public string Name { get; private set; }

        public InjectComponent(string name)
        {
            Name = name;
        }

        public InjectComponent(Object name) : this(name.ToString())
        {
        }
    }
}