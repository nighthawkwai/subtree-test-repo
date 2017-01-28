using System;
using System.Collections.Concurrent;

namespace Microsoft.Services.Core
{
    /// <summary>
    /// An extensions class for operating with <see cref="StringValueAttribute"/>
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
    public static class StringValueAttributeExtensions
    {
        private static readonly ConcurrentDictionary<Enum, StringValueAttribute> AttributeCache  = new ConcurrentDictionary<Enum, StringValueAttribute>();

        public static string ToStringValue(this Enum value)
        {
            var attr =   GetAttribute(value);
            return attr != null ? attr.Value : value.ToString();
        }

        private static StringValueAttribute GetAttribute(Enum value)
        {
            return AttributeCache.GetOrAdd(value, (@enum) =>
            {
                var attrs =
                    (StringValueAttribute[])
                        value.GetType()
                            .GetField(value.ToString())
                            .GetCustomAttributes(typeof(StringValueAttribute), false);
                return attrs.Length > 0 ? attrs[0] : null;
            });
        }
    }
}