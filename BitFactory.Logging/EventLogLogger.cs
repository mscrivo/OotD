/* 
 * (C) Copyright 2002, 2005, 2009 - Lorne Brinkman - All Rights Reserved.
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
using System.Diagnostics;

namespace BitFactory.Logging
{
	/// <summary>
	/// EventLogLogger writes log entries to the Windows Event Log
	/// </summary>
	public class EventLogLogger : Logger
	{
		/// <summary>
		/// The Windows event log.
		/// </summary>
		private EventLog _eventLog;
		/// <summary>
		/// Gets and Sets the Windows event log.
		/// </summary>
		protected EventLog EventLog 
		{
			get { return _eventLog; }
			set { _eventLog = value; }
		}

		/// <summary>
		/// Write aLogEntry information to the Windows event log
		/// </summary>
		/// <param name="aLogEntry">The LogEntry being logged</param>
		/// <returns>true upon success, false upon failure.</returns>
		protected internal override bool DoLog(LogEntry aLogEntry) 
		{
			// convert the log entry's severity to an appropriate type for the event log
			EventLogEntryType t =
				(aLogEntry.Severity < LogSeverity.Warning) ?
					EventLogEntryType.Information :
					(aLogEntry.Severity == LogSeverity.Warning ?
						EventLogEntryType.Warning :
						EventLogEntryType.Error);

			try
			{
				EventLog.WriteEntry( Formatter.AsString(aLogEntry), t );
			} 
			catch(Exception ex)
			{
                OnLoggingError(this, "Error writing to EventLog", ex);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Create a new instance of EventLogLogger
		/// </summary>
		public EventLogLogger() : base()
		{
			EventLog = new EventLog();
			EventLog.Source = AppDomain.CurrentDomain.FriendlyName;
		}
        /// <summary>
        /// Create a new instance of EventLogLogger with a given Eventog
        /// </summary>
        /// <param name="anEventLog">An EventLog to which logs will be written by this logger</param>
        public EventLogLogger(EventLog anEventLog) : base()
        {
            EventLog = anEventLog;
        }

		/// <summary>
		/// EventLogLogger uses LogEntryMessageOnlyFormatter by default.
		/// </summary>
		/// <returns></returns>
		protected override LogEntryFormatter GetDefaultFormatter() 
		{
			return new LogEntryMessageOnlyFormatter(OnLoggingError);
		}
	}
}
