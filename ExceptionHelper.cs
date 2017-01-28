using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Services.Core
{
    /// <summary>
    /// A bunch of generic exception helper methods to help users search through and format exception strings
    /// </summary>
    public static class ExceptionHelper
    {
        /// <summary>
        /// Finds the first type of the exception of <typeparam name="T"/> in the exception tree.
        /// Returns null if not found
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="e">The e.</param>
        /// <returns></returns>
        public static T FindFirstExceptionOfType<T>(Exception e)
            where T : Exception
        {
            if (e == null)
            {
                return null;
            }
            for (Exception ex = e; ex != null; ex = ex.InnerException)
            {
                if (ex is T)
                {
                    return (T)ex;
                }
            }
            return null;
        }



        /// <summary>
        /// Gets a tab indented exception string of the form
        /// <c>
        ///     "An exception has occurred: ...
        ///         Stacktrace:
        ///          An Exception has occurred: ... 
        ///             Stacktrace:
        ///          An Exception has occurred: ... 
        ///             Stacktrace:
        /// </c>
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="tabIndent">The tab indent.</param>
        /// <param name="traverseExceptionTree">if set to <c>true</c> [traverse exception tree].</param>
        /// <returns></returns>
        public static string GetIndentedExceptionString(Exception e, int tabIndent = 0, bool traverseExceptionTree = false)
        {
            if (e == null)
                return string.Empty;
            List<Exception> allTopLevelExceptionsToProcess = new List<Exception> { e };
            AggregateException ae = e as AggregateException;
            if (ae != null)
            {
                allTopLevelExceptionsToProcess.AddRange(ae.InnerExceptions);
            }
            StringBuilder retVal = new StringBuilder();
            foreach (var topLevelExceptionToProcess in allTopLevelExceptionsToProcess)
            {
                List<Exception> allInnerExceptionsToProcess = new List<Exception> { topLevelExceptionToProcess };

                if (traverseExceptionTree)
                {
                    for (Exception inner = e.InnerException; inner != null; inner = inner.InnerException)
                    {
                        allInnerExceptionsToProcess.Add(inner);
                    }
                }
                for (int i = 0; i < allInnerExceptionsToProcess.Count; i++)
                {
                    GetIndentedExceptionString(allInnerExceptionsToProcess[i], tabIndent + i, retVal);
                }
            }
            return retVal.ToString();
        }

        private static void GetIndentedExceptionString(Exception e, int tabIndent, StringBuilder retVal)
        {
            string tabPadding = string.Join(string.Empty, Enumerable.Repeat("\t", tabIndent));
            retVal.AppendLine(tabPadding + string.Format("An exception has occurred: {0}", e.Message));
            retVal.AppendLine(tabPadding + string.Format("\tStack trace: {0}", e.StackTrace));
            retVal.AppendLine();
        }
    }
}
