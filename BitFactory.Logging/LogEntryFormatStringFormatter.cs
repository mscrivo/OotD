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
    /// A LogEntryFormatter subclass that works like the string.Format() method.
    /// Instead of using an index for the parameter, use certain reserved names, such as:
    /// application
    /// machine
    /// category
    /// severity
    /// timestamp
    /// message
    /// eg. "{timestamp:G}..{severity}..{message}"
    /// </summary>
    public class LogEntryFormatStringFormatter : LogEntryFormatter
    {
        /// <summary>
        /// The string representing the format of the log strings as they should be output to the log
        /// </summary>
        public new readonly string FormatString;

        /// <summary>
        /// Instantiate a new instance of LogEntryFormatStringFormatter
        /// </summary>
        /// <param name="aFormatString">The format string</param>
        /// <param name="aLoggingErrorHandler">A delegate used to trigger the LoggingError event in a Logger</param>
        public LogEntryFormatStringFormatter(string aFormatString, Logger.LoggingErrorHandler aLoggingErrorHandler) : base(aLoggingErrorHandler)
        {
            FormatString = aFormatString;
        }

        /// <summary>
        /// return a LogEntry formatted as a string
        /// </summary>
        /// <param name="aLogEntry">The LogEntry to log</param>
        /// <returns>a string formatted appropriately</returns>
        protected internal override string AsString(LogEntry aLogEntry)
        {
            return LogEntryRegexMatchReplacer.Replace(FormatString, aLogEntry, LoggingErrorHandler);
        }

    }
}
