using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.Services.Core
{
    /// <summary>
    /// A contracts class that mimics the functionality of <see cref="System.Diagnostics.Contracts"/>
    /// </summary>
    public static class Contract
    {
        private static string ConstructLocationMessage(string memberName, string sourceFilePath, int sourceLineNumber)
        {
            return $"{memberName} at {sourceFilePath}:{sourceLineNumber}";
        }
        /// <summary>
        /// Wrapper for the CodeContract Requries method
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="condition"></param>
        /// <param name="message"></param>
        public static void Requires<T>(bool condition, 
            string message = null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0) where T : Exception
        {
            if (!condition)
            {
                // Find the constructor that has a String param
                Type type = typeof(T);
                ConstructorInfo cinfo = type.GetConstructor(new Type[] { typeof(String) });
                if (cinfo == null)
                {
                    throw default(T);
                }

                throw (Exception)cinfo.Invoke(new object[]
                    {
                        (message ?? "Requires condition failed") + " - " + ConstructLocationMessage(memberName, sourceFilePath, sourceLineNumber)
                    });
            }
        }

        /// <summary>
        /// Asserts that arg is not null.
        /// </summary>
        /// <param name="arg">The arg.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void AssertNotNull(object arg, string name, string message=null,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(name,
                    message ?? string.Empty + " - " + ConstructLocationMessage(memberName, sourceFilePath, sourceLineNumber));
            }
        }

        /// <summary>
        /// Asserts that arg is not null.
        /// </summary>
        /// <param name="arg">The arg.</param>
        /// <param name="argname">The argname.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static void AssertArgNotNull(object arg, string argname, 
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argname, ConstructLocationMessage(memberName, sourceFilePath, sourceLineNumber));
            }
        }

        /// <summary>
        /// Asserts that arg is not null or empty or WhiteSpace.
        /// </summary>
        /// <param name="arg">The arg.</param>
        /// <param name="argname">The argname.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        /// <exception cref="System.ArgumentException">Argument cannot be empty</exception>
        public static void AssertArgNotNullOrEmptyOrWhitespace(string arg, string argname,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string sourceFilePath = "",
            [CallerLineNumber] int sourceLineNumber = 0)
        {
            if (arg == null)
            {
                throw new ArgumentNullException(argname);
            }

            if (string.IsNullOrWhiteSpace(arg))
            {
                throw new ArgumentException("Argument cannot be empty whitespace - " + ConstructLocationMessage(memberName, sourceFilePath,sourceLineNumber), argname);
            }
        }
    }
}
