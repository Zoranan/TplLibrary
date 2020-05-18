using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TplLib.Tpl_Parser.Attributes
{
    /// <summary>
    /// Marks a class as a TplFunction, and specifies its function name in the Tpl syntax
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class TplFunctionAttribute : Attribute
    {
        public string Name { get; }

        public TplFunctionAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TplPositionalArgumentAttribute : Attribute
    {
        public int Position { get; }
        public object DefaultValue { get; }

        public TplPositionalArgumentAttribute(int position, object defaultValue)
        {
            Position = position;
            DefaultValue = defaultValue;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class TplNamedArgumentAttribute : Attribute
    {
        public string Name { get; }
        public object DefaultValue { get; }

        public TplNamedArgumentAttribute(string name, object defaultValue)
        {
            Name = name;
            DefaultValue = defaultValue;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TplInitMethodAttribute : Attribute
    { }
}
