using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Services.Core
{
    /// <summary>
    /// String value attribute to decorate enum fields
    /// </summary>
    /// <example>
    /// Here is a minimal example of its usage
    /// <code>
    /// public enum TestEnum
    ///  {
    ///    [StringValue("Duck")]
    ///    Value1,
    ///    [StringValue("Dog")]
    ///    Value2,
    ///    [StringValue("Cow")]
    ///    Value3
    ///  }
    /// 
    /// void Main()
    /// {
    ///    TestEnum.Value1.ToStringValue()
    /// }
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class StringValueAttribute : Attribute
    {
        public string Value { get; private set; }

        public StringValueAttribute(string value)
        {
            this.Value = value;
        }
    }
}
