/* 
 * (C) Copyright 2009 - Lorne Brinkman - All Rights Reserved.
 * http://www.TheObjectGuy.com
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT,
 * INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA,
 * OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 * 
 */

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace BitFactory.Logging
{
    /// <summary>
    /// This class formats strings using a format string with named parameters, with data from a LogEntry.
    /// variables to replace are:
    /// application
    /// machine
    /// category
    /// severity
    /// timestamp
    /// message
    /// A format string might look like: "{timestamp:G}..{severity}..{message}"
    /// </summary>
    public abstract class LogEntryRegexMatchReplacer
    {
        /// <summary>
        /// The set of items that can be formatted
        /// </summary>
        public enum EVariable
        {
            /// <summary>
            /// Application
            /// </summary>
            Application,
            /// <summary>
            /// Machine
            /// </summary>
            Machine,
            /// <summary>
            /// Category
            /// </summary>
            Category,
            /// <summary>
            /// Severity
            /// </summary>
            Severity,
            /// <summary>
            /// Timestamp
            /// </summary>
            Timestamp,
            /// <summary>
            /// Message
            /// </summary>
            Message
        }

        private const string RegexPattern = @"\{[VariableName](,.*?)?(:.*?)?\}";
        
        /// <summary>
        /// The item being formatted
        /// </summary>
        protected EVariable Variable { get; set; }

        /// <summary>
        /// The LogEntry to use
        /// </summary>
        protected LogEntry LogEntry { get; set; }

        /// <summary>
        /// A delegate used to trigger the LoggingError event in a logger
        /// </summary>
        protected Logger.LoggingErrorHandler LoggingErrorHandler { get; set; }

        private string DoReplace(Match aMatch)
        {            
            var formatString = Regex.Replace(aMatch.Value, Variable.ToString(), "0", RegexOptions.IgnoreCase);
            try
            {
                return string.Format(formatString, VariableValue);
            }
            catch(Exception ex)   // can't format it, so just return the VariableValue--which may not even be in a valid format
            {
                LoggingErrorHandler("Formatting error", ex);
                return VariableValue.ToString();
            }
        }

        /// <summary>
        /// The replacement for the variable
        /// </summary>
        protected abstract object VariableValue { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        protected LogEntryRegexMatchReplacer()
        {
        }

        private static LogEntryRegexMatchReplacer ForVariable(EVariable aVariable, LogEntry aLogEntry, Logger.LoggingErrorHandler aLoggingErrorHandler)
        {
            LogEntryRegexMatchReplacer mr = null;
            switch (aVariable)
            {
                case EVariable.Application:
                    mr = new ApplicationReplacer();
                    break;
                case EVariable.Machine:
                    mr = new MachineReplacer();
                    break;
                case EVariable.Category:
                    mr = new CategoryReplacer();
                    break;
                case EVariable.Severity:
                    mr = new SeverityReplacer();
                    break;
                case EVariable.Timestamp:
                    mr = new TimestampReplacer();
                    break;
                case EVariable.Message:
                    mr = new MessageReplacer();
                    break;
            }
            mr.Variable = aVariable;
            mr.LogEntry = aLogEntry;
            mr.LoggingErrorHandler = aLoggingErrorHandler;
            return mr;
        }

        /// <summary>
        /// Utility method to do the replacing in a format string
        /// </summary>
        /// <param name="aFormatString">The format string (e.g. "{timestamp:G}..{severity}..{message}")</param>
        /// <param name="aLogEntry">The LogEntry</param>
        /// <param name="aLoggingErrorHandler">A delegate used to trigger the LoggingError event in a logger</param>
        /// <returns>A formatted string</returns>
        public static string Replace(string aFormatString, LogEntry aLogEntry, Logger.LoggingErrorHandler aLoggingErrorHandler)
        {
            var newString = aFormatString;
            foreach (EVariable variable in Enum.GetValues(typeof(EVariable)))
            {
                var regex = new Regex(RegexPattern.Replace("[VariableName]", variable.ToString()), RegexOptions.IgnoreCase);
                newString = regex.Replace(newString, LogEntryRegexMatchReplacer.ForVariable(variable, aLogEntry, aLoggingErrorHandler).DoReplace);
            }
            return newString;
        }

        /// <summary>
        /// Return true if the given aFormatString matches the regex for the given variable
        /// </summary>
        /// <param name="aFormatString">The format string to check for a match</param>
        /// <param name="aVariable">The variable to check</param>
        /// <returns></returns>
        public static bool IsMatch(string aFormatString, EVariable aVariable)
        {
            var regex = new Regex(RegexPattern.Replace("[VariableName]", aVariable.ToString()), RegexOptions.IgnoreCase);
            return regex.IsMatch(aFormatString);
        }

        private class ApplicationReplacer : LogEntryRegexMatchReplacer
        {
            protected override object VariableValue
            {
                get { return LogEntry.Application; }
            }
        }

        private class MachineReplacer : LogEntryRegexMatchReplacer
        {
            protected override object VariableValue
            {
                get { return Environment.MachineName; }
            }
        }

        private class CategoryReplacer : LogEntryRegexMatchReplacer
        {
            protected override object VariableValue
            {
                get { return LogEntry.Category; }
            }
        }

        private class SeverityReplacer : LogEntryRegexMatchReplacer
        {
            protected override object VariableValue
            {
                get { return LogEntry.Severity; }
            }
        }

        private class TimestampReplacer : LogEntryRegexMatchReplacer
        {
            protected override object VariableValue
            {
                get { return LogEntry.Date; }
            }
        }

        private class MessageReplacer : LogEntryRegexMatchReplacer
        {
            protected override object VariableValue
            {
                get { return LogEntry.Message; }
            }
        }


    }
}
