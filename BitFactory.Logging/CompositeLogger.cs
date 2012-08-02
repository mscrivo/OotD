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
using System.Collections;

namespace BitFactory.Logging
{
	/// <summary>
	/// A CompositeLogger contains other Logger instances.
	/// </summary>
	/// <remarks>
	/// When instances of this class log LogEntries,
	/// they simply pass on the LogEntries to the Loggers contained therein.
	///
	/// Loggers are contained in a HashMap, with the keys being Strings.
	/// This provides a nice means to access a specific contained Logger, if necessary.
	///
	/// Instances of this class are likely to be used as an application's main logger,
	/// within which more specific loggers can be contained.
	/// </remarks>
	public class CompositeLogger : Logger
	{
		/// <summary>
		/// The Collection of Loggers.
		/// </summary>
		private IDictionary _loggers = new Hashtable();
		
		/// <summary>
		/// Gets and sets the Collection of Loggers.
		/// </summary>
		protected IDictionary Loggers 
		{
			get { return _loggers; }
			set { _loggers = value; }
		}

		/// <summary>
		/// Add a Logger to this CompositeLogger.
		/// </summary>
		/// <param name="aName">A meaningful name for possible access later.</param>
		/// <param name="aLogger">The Logger to add.</param>
		public void AddLogger(String aName, Logger aLogger) 
		{
            lock (this) // lock in case loggers are added dynamically in a background thread
            {
                Loggers.Add(aName, aLogger);
            }
		}

		/// <summary>
		/// Remove a Logger
		/// </summary>
		/// <param name="aName">The name of the Logger to remove</param>
		public void RemoveLogger(String aName) 
		{
            lock (this) // lock in case loggers are removed dynamically in a background thread
            {
                Loggers.Remove(aName);
            }
		}

		/// <summary>
		/// Send the log message to contained Loggers.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry to log.</param>
		/// <returns>Always return true--assume success</returns>
		protected internal override bool DoLog(LogEntry aLogEntry) 
		{
            foreach (Logger logger in Loggers.Values)
                logger.Log(aLogEntry);
			return true;
		}

		/// <summary>
		/// Gets the Logger with the name corresponding to the given String.
		/// </summary>
		/// <param name="aName">The name of the desired Logger</param>
		/// <returns>The Logger corresponding with aName</returns>
		public Logger this[String aName] 
		{
			get { return (Logger) Loggers[aName]; }
		}

        /// <summary>
        /// Search recursively for the first logger with the given name. The search will include all descendents (i.e. also search within child CompositeLoggers)
        /// </summary>
        /// <param name="aName">The name of the logger to find</param>
        /// <returns>The first matching logger, or null if not found</returns>
        public Logger FindLogger(string aName)
        {
            var logger = this[aName];
            if (logger != null)
                return logger;

            foreach (DictionaryEntry entry in Loggers)
                if (entry.Value is CompositeLogger)
                {
                    logger = ((CompositeLogger)entry.Value).FindLogger(aName);
                    if (logger != null)
                        return logger;
                }

            return null;
        }
		

		/// <summary>
		/// Create a new instance of CompositeLogger
		/// </summary>
		public CompositeLogger() : base()
		{
		}
	}
}
