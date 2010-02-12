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

namespace BitFactory.Logging
{
	/// <summary>
	/// Concrete subclasses of this abstract class do the actual logging of information
	/// to their respective "logs."
	/// </summary>
	/// <remarks>
	/// Subclasses that simply write a String to a log need only override WriteToLog(String).
	/// Subclasses that do a little more processing may prefer to override DoLog(LogEntry).
	///
	/// In general, users of the various Logger subclasses need to be aware of only the
	/// public Log*(Object anItemToLog) and Log*(Object aCategory, Object anItemToLog) methods
	///	(where * is one of the severities, such as LogInfo( "This is information" ) ).
	/// You can log any Object, and it's ToString() will be used in the LogEntry. Optionally, you
	/// can use categories to distinguish various types of items being logged. These would usually
	/// be Strings, but can be any Object. e.g. To log all database related items, you could
	/// do something like LogWarning("DB System", "Cannot connect to DB"); where you categorize
	/// all database logs with "DB System". A LogEntryCatagegoryFilter subclass could optionally
	/// be used to log only database related items.
	///
	/// application - This optional String represents information that is application-wide.
	/// It might be a user name, or it might be the name of the application.
	/// </remarks>
	/// <seealso cref="LogEntryCategoryFilter"/>

	public abstract class Logger
	{
        /// <summary>
        /// Class to support errors that occur during logging
        /// </summary>
        public class LoggingErrorEventArgs : EventArgs
        {
            /// <summary>
            /// A string with extra information about a logging error
            /// </summary>
            public readonly string Info;
            /// <summary>
            /// The Exception thrown while logging
            /// </summary>
            public readonly Exception Exception;
            /// <summary>
            /// Create a new instance
            /// </summary>
            /// <param name="info">A string with info about the error</param>
            /// <param name="ex">The exception thrown during logging</param>
            public LoggingErrorEventArgs(string info, Exception ex)
            {
                Info = info;
                Exception = ex;
            }
        }

        /// <summary>
        /// An event that occurs when an Exception occurs within the logging system.
        /// This can be used to help diagnose logging problems. (For example, file permission issues for a FileLogger.)
        /// Under most conditions, it is not necessary to use this event.
        /// </summary>
        public static event EventHandler<LoggingErrorEventArgs> LoggingError;

        /// <summary>
        /// Called when an Exception occurs in the logging system
        /// </summary>
        /// <param name="aLogger">The Logger for which the Exception occured</param>
        /// <param name="info">A string with extra information regarding the error</param>
        /// <param name="ex">The Exception that was thrown</param>
        protected static void OnLoggingError(Logger aLogger, string info, Exception ex)
        {
            if (LoggingError != null)
                try
                {
                    LoggingError(aLogger, new LoggingErrorEventArgs(info, ex));
                }
                catch
                {
                }
        }

        /// <summary>
        /// A convenience method called by Logger instances
        /// </summary>
        /// <param name="info">A string with extra information regarding the error</param>
        /// <param name="ex">The Exception that was thrown</param>
        protected internal void OnLoggingError(string info, Exception ex)
        {
            OnLoggingError(this, info, ex);
        }

        /// <summary>
        /// A delegate that can be used by satelite classes to trigger the LoggingError event
        /// </summary>
        /// <param name="info">A string with extra information regarding the error</param>
        /// <param name="ex">The Exception that was thrown</param>
        public delegate void LoggingErrorHandler(string info, Exception ex);


		/// <summary>
		/// A String representing the application.
		/// </summary>
		private String _application;
		/// <summary>
		/// A bool indicating if this Logger is enabled or not.
		/// </summary>
		private bool _enabled = true;
		/// <summary>
		/// The filter through which all LogEntries must pass before being logged.
		/// </summary>
		private LogEntryFilter _filter;
		/// <summary>
		/// A formatter that formats LogEntries before being logged.
		/// </summary>
		private LogEntryFormatter _formatter;
		/// <summary>
		/// The default class to use for formatting.
		/// </summary>
		private static Type _defaultFormatterClass = typeof(LogEntryStandardFormatter);
		/// <summary>
		/// The default class to use for filtering.
		/// </summary>
		private static Type _defaultFilterClass = typeof(LogEntryPassFilter);

		/// <summary>
		/// Utility property to simply indicate the version of the Logging assembly being used.
		/// </summary>
		public static Version Version { 
			get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version; }
		}

		/// <summary>
		/// Gets and sets the Application.
		/// </summary>
		public String Application 
		{
			get { return _application; }
			set { _application = value; }
		}
		/// <summary>
		/// Gets and sets the Enabled flag.
		/// </summary>
		public bool Enabled 
		{
			get { return _enabled; }
			set { _enabled = value; }
		}
		/// <summary>
		/// Gets and sets the Filter.
		/// </summary>
		public LogEntryFilter Filter 
		{
			get { return _filter; }
			set { _filter = value; }
		}
		/// <summary>
		/// Gets and sets the Formatter.
		/// </summary>
		public LogEntryFormatter Formatter 
		{
			get { return _formatter; }
			set { _formatter = value; }
		}
		/// <summary>
		/// Gets and sets the DefaultFormatterClass.
		/// </summary>
		public static Type DefaultFormatterClass 
		{
			get { return _defaultFormatterClass; }
			set { _defaultFormatterClass = value; }
		}
		/// <summary>
		/// Gets and sets the DefaultFilterClass.
		/// </summary>
		public static Type DefaultFilterClass 
		{
			get { return _defaultFilterClass; }
			set { _defaultFilterClass = value; }
		}
		/// <summary>
		/// Logger constructor.
		/// </summary>
		protected Logger() : base()
		{
			Filter = GetDefaultFilter();
			Formatter = GetDefaultFormatter();
		}

		/// <summary>
		/// Really log aLogEntry. (We are past filtering at this point.)
		/// Subclasses might want to do something more interesting and override this method.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry to log</param>
		/// <returns>true upon success, false upon failure.</returns>
		protected internal virtual bool DoLog(LogEntry aLogEntry) 
		{
			return WriteToLog( Formatter.AsString( aLogEntry ) );
		}
		/// <summary>
		/// Gets and sets the severity threshold of the filter--the lowest severity which will be logged.
		/// </summary>
		public LogSeverity SeverityThreshold 
		{
			get { return Filter.SeverityThreshold; }
			set { Filter.SeverityThreshold = value; }
		}
        /// <summary>
        /// Create a LogEntry.
        /// </summary>
        /// <remarks>
        /// This is virtual and can be overriden in subclasses that,
        /// although unlikely, might want to create a subclass of LogEntry.
        /// </remarks>
        /// <param name="aSeverity">The severity of this log entry.</param>
        /// <param name="aCategory">An Object (often a String) that classifies this log entry.</param>
        /// <param name="anObject">An Object (often a String) to log.</param>
        /// <returns>A LogEntry</returns>
        protected virtual LogEntry CreateLogEntry(LogSeverity aSeverity, Object aCategory, Object anObject)
        {
            String logString;
            try
            {
                logString = anObject.ToString();
            }
            catch (Exception ex)
            {
                OnLoggingError(this, "Error converting to string", ex);
                logString = "Unknown... " + ex.Message;
            }
            return new LogEntry(aSeverity, Application, aCategory, logString);
        }
		/// <summary>
		/// Log something.
		/// </summary>
		/// <param name="aSeverity">The severity of this log entry.</param>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void Log(LogSeverity aSeverity, Object anObject)
		{
			Log( aSeverity, null, anObject );
		}
		/// <summary>
		/// Log something.
		/// </summary>
		/// <param name="aSeverity">The severity of this log entry.</param>
		/// <param name="aCategory">An Object (often a String) that classifies this log entry.</param>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void Log(LogSeverity aSeverity, Object aCategory, Object anObject)
		{
			Log( CreateLogEntry( aSeverity, aCategory, anObject ));
		}
		/// <summary>
		/// Log something.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry.</param>
		/// <returns>true upon success, false upon failure.</returns>
		protected internal bool Log(LogEntry aLogEntry)
		{
			lock(this) 
			{
                try
                {
                    return ShouldLog(aLogEntry) ? DoLog(aLogEntry) : true;
                }
                catch (Exception ex)
                {
                    OnLoggingError(this, "", ex);
                    return false;
                }
			}
		}
		/// <summary>
		/// Make a "critical" log entry.
		/// </summary>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogCritical(Object anObject)
		{
			Log( LogSeverity.Critical, anObject);
		}
		/// <summary>
		/// Make a "Critical" log entry.
		/// </summary>
		/// <param name="aCategory">An Object (often a String) that classifies this log entry.</param>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogCritical(Object aCategory, Object anObject)
		{
			Log( LogSeverity.Critical, aCategory, anObject);
		}
		/// <summary>
		/// Make a "Debug" log entry.
		/// </summary>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogDebug(Object anObject)
		{
			Log( LogSeverity.Debug, anObject);
		}
		/// <summary>
		/// Make a "Debug" log entry.
		/// </summary>
		/// <param name="aCategory">An Object (often a String) that classifies this log entry.</param>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogDebug(Object aCategory, Object anObject)
		{
			Log( LogSeverity.Debug, aCategory, anObject);
		}
		/// <summary>
		/// Make an "Error" log entry.
		/// </summary>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogError(Object anObject)
		{
			Log( LogSeverity.Error, anObject);
		}
		/// <summary>
		/// Make an "Error" log entry.
		/// </summary>
		/// <param name="aCategory">An Object (often a String) that classifies this log entry.</param>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogError(Object aCategory, Object anObject)
		{
			Log( LogSeverity.Error, aCategory, anObject);
		}
		/// <summary>
		/// Make a "Fatal" log entry.
		/// </summary>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogFatal(Object anObject)
		{
			Log( LogSeverity.Fatal, anObject);
		}
		/// <summary>
		/// Make a "Fatal" log entry.
		/// </summary>
		/// <param name="aCategory">An Object (often a String) that classifies this log entry.</param>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogFatal(Object aCategory, Object anObject)
		{
			Log( LogSeverity.Fatal, aCategory, anObject);
		}
		/// <summary>
		/// Make an "Info" log entry.
		/// </summary>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogInfo(Object anObject)
		{
			Log( LogSeverity.Info, anObject);
		}
		/// <summary>
		/// Make an "Info" log entry.
		/// </summary>
		/// <param name="aCategory">An Object (often a String) that classifies this log entry.</param>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogInfo(Object aCategory, Object anObject)
		{
			Log( LogSeverity.Info, aCategory, anObject);
		}
		/// <summary>
		/// Make an "Status" log entry.
		/// </summary>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogStatus(Object anObject)
		{
			Log( LogSeverity.Status, anObject);
		}
		/// <summary>
		/// Make an "Status" log entry.
		/// </summary>
		/// <param name="aCategory">An Object (often a String) that classifies this log entry.</param>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogStatus(Object aCategory, Object anObject)
		{
			Log( LogSeverity.Status, aCategory, anObject);
		}
		/// <summary>
		/// Make an "Warning" log entry.
		/// </summary>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogWarning(Object anObject)
		{
			Log( LogSeverity.Warning, anObject);
		}
		/// <summary>
		/// Make an "Warning" log entry.
		/// </summary>
		/// <param name="aCategory">An Object (often a String) that classifies this log entry.</param>
		/// <param name="anObject">An Object (often a String) to log.</param>
		public void LogWarning(Object aCategory, Object anObject)
		{
			Log( LogSeverity.Warning, aCategory, anObject);
		}
		/// <summary>
		/// Create a new filter instance to use for this Logger.
		/// Use the DefaultFilterClass property.
		/// Subclasses may wish to override.
		/// </summary>
		/// <returns>A new filter.</returns>
		protected virtual LogEntryFilter GetDefaultFilter() 
		{
			LogEntryFilter aFilter = null;
			try 
			{	
				aFilter = (LogEntryFilter) DefaultFilterClass.GetConstructor(new Type[0]).Invoke(new Object[0]);
			}
			catch(Exception ex)
			{
                OnLoggingError(this, "Error creating filter", ex);
				aFilter = new LogEntryPassFilter();
			}
			return aFilter;
		}
		/// <summary>
		/// Create a new formatter instance to use for this Logger.
		/// Use the DefaultFormatterClass property.
		/// Subclasses may wish to override.
		/// </summary>
		/// <returns>A new formatter.</returns>
		protected virtual LogEntryFormatter GetDefaultFormatter() 
		{
			LogEntryFormatter aFormatter = null;
			try 
			{
				aFormatter = (LogEntryFormatter) DefaultFormatterClass.GetConstructor(new Type[] {typeof(LoggingErrorHandler)}).Invoke(new Object[] { new LoggingErrorHandler(OnLoggingError) });
			}
			catch (Exception ex) 
			{
                OnLoggingError(this, "Error creating formatter", ex);
				aFormatter = new LogEntryStandardFormatter(OnLoggingError);
			}
			return aFormatter;
		}
		/// <summary>
		/// Determine if aLogEntry should be logged
		/// </summary>
		/// <param name="aLogEntry">The LogEntry to test.</param>
		/// <returns>true if aLogEntry should be logged.</returns>
		private bool ShouldLog(LogEntry aLogEntry)
		{
			return Enabled && Filter.ShouldLog( aLogEntry );
		}
		/// <summary>
		/// Write a String to the log.
		/// Subclasses can override this to actually write to the log.
		/// </summary>
		/// <param name="s">The String</param>
		/// <returns>true upon success, false upon failure.</returns>
		protected virtual bool WriteToLog(String s) 
		{
			return true;
		}
	}
}
