/* 
 * (C) Copyright 2005, 2009 - Lorne Brinkman - All Rights Reserved.
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
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace BitFactory.Logging
{
	/// <summary>
	/// RollingFileLogger can be used to automatically roll-over files by any determination.
    /// It does not delete old log files, it just creates new log files as required.
    /// </summary>
    /// <remarks>
    /// A log file name can change based on the current date, such as MyLogFile_20050615.log, MyLogFile_20050616.log.
	/// Or at a predetermined size, the log file name can automatically increment, such as MyLogFile_1.log, MyLogFile_2.log.
	/// There are a couple included inner 'Strategy' classes defined for rolling-over files
	/// based on the date or the size of the log. Others can easily be added.
    /// </remarks>
    public class RollingFileLogger : FileLogger
    {
        #region Attributes

        private readonly RollOverStrategy FileRollOverStrategy;

        #endregion

        #region logging

		/// <summary>
		/// Do Log aLogEntry
		/// </summary>
		/// <param name="aLogEntry">A LogEntry</param>
		/// <returns>true if successfully logged, otherwise false</returns>
        protected internal override bool DoLog(LogEntry aLogEntry)
        {
            string formattedLogString = Formatter.AsString( aLogEntry );
            FileName = FileRollOverStrategy.GetFileName(aLogEntry, formattedLogString);
            return WriteToLog(formattedLogString);
        }

        #endregion

        #region Initialization

		/// <summary>
		/// Instantiate a new RollingFileLogger with the given RollOverStrategy
		/// </summary>
		/// <param name="aRollOverStrategy">A RollOverStrategy the logger will use to determine when it should roll-over</param>
        public RollingFileLogger(RollOverStrategy aRollOverStrategy) : base()
        {
            FileRollOverStrategy = aRollOverStrategy;
            FileRollOverStrategy.LoggingErrorHandler = OnLoggingError;
        }

		/// <summary>
		/// Create and return a new RollingFileLogger that rolls over according to the current date.
		/// </summary>
		/// <param name="aFullPathFormatString">The Full path of the log file with a parameter ({timestamp:yyyyMMdd}) for the variable date, for example: "\mylogs\logfile{timestamp:yyyyMMdd}.log"</param>
		/// <returns>A new RollingFileLogger</returns>
        public static RollingFileLogger NewRollingDateFileLogger(string aFullPathFormatString)
        {
            return new RollingFileLogger(new RollOverDateStrategy(aFullPathFormatString));
        }

        /// <summary>
        /// Create and return a new RollingFileLogger that rolls over according to the format string
        /// </summary>
        /// <param name="aFullPathFormatString">The Full path of the log file with parameters for any variables of the LogEntries being logged. For example: "\mylogs\logfile{timestamp:yyyyMMdd}_{category}.log"</param>
        /// <returns>A new RollingFileLogger</returns>
        public static RollingFileLogger NewRollingFileLogger(string aFullPathFormatString)
        {
            return new RollingFileLogger(new RollOverFormatStrategy(aFullPathFormatString));
        }

		/// <summary>
		/// Create and return a new RollingFileLogger that rolls over according to the size of the log file
		/// </summary>
		/// <param name="aFullPathFormatString">The Full path of the log file with a parameter ({0}) for a variable number (1, 2, 3, etc.), for example: "\mylogs\logfile{0}.log"</param>
		/// <param name="aMaxSize">The maximum size the file should be before it rolls over to the next file</param>
		/// <returns>A new RollingFileLogger</returns>
        public static RollingFileLogger NewRollingSizeFileLogger(string aFullPathFormatString, long aMaxSize)
        {
            return new RollingFileLogger(new RollOverSizeStrategy(aFullPathFormatString, aMaxSize));
        }

        #endregion

        #region Roll-over strategy classes

		/// <summary>
		/// RollOverStrategy is an abstract class that defines the basic functionality required by a RollingFileLogger to roll-over.
		/// </summary>
        public abstract class RollOverStrategy
        {
            #region Attributes

            /// <summary>
            /// A deleate used to trigger the LoggingError event in a logger
            /// </summary>
            public Logger.LoggingErrorHandler LoggingErrorHandler { get; set; }

			/// <summary>
			/// The format string used to generate the log file name
			/// </summary>
            protected readonly string FileNameFormatString;

            #endregion

            #region Initialization

			/// <summary>
			/// Instantiate a RollOverStrategy providing a string looking like "c:\SomeDirectoryPath\SomeFileName{0}.log"
			/// </summary>
			/// <remarks>
			/// A FormatException will be thrown if the provided string does not include the format item "{0}"
			/// </remarks>
			/// <param name="aFullPathFormatString">A string describing the full path of the log file with a format item (e.g. "c:\SomeDirectoryPath\SomeFileName{0}.log")</param>
            public RollOverStrategy(string aFullPathFormatString)
            {
                ValidateFormatString(aFullPathFormatString);
                FileNameFormatString = aFullPathFormatString;
            }

            #endregion

            /// <summary>
            /// Validate the format of the file name
            /// </summary>
            /// <param name="aFileNameFormatString">The format string of the file name</param>
            protected virtual void ValidateFormatString(string aFileNameFormatString)
            {
                if (aFileNameFormatString.IndexOf("{0}") == -1)
                    throw new FormatException("This RollOverStrategy FileNameFormatString must contain a format item \"{0}\"");
            }

            /// <summary>
            /// Return the name of the log file
            /// </summary>
            /// <param name="aLogEntry">The LogEntry being logged</param>
            /// <param name="aFormattedLogString">The string being written to the log</param>
            /// <returns>A file name formatted accoring to FileNameFormatString</returns>
            protected internal virtual string GetFileName(LogEntry aLogEntry, string aFormattedLogString)
            {
                return string.Format(FileNameFormatString, GetIncrementalName(aLogEntry, aFormattedLogString));
            }

            /// <summary>
            /// Return the variable portion of the name of the log file
            /// </summary>
            /// <param name="aLogEntry">The LogEntry being logged</param>
            /// <param name="aFormattedLogString">The string being written to the log</param>
            /// <returns>The variable portion of the name of the log file</returns>
            protected internal abstract string GetIncrementalName(LogEntry aLogEntry, string aFormattedLogString);
        }

        /// <summary>
        /// This class is a general replacement for RollOverDateStrategy.
        /// It can replicate RollOverDateStrategy simply by using a format string like "MyLog_{timestamp:yyydMMdd}.log"
        /// This class can be used to put items of different categories into different files also, using a format string like: "MyLog_{category}.log"
        /// </summary>
        public class RollOverFormatStrategy : RollOverStrategy
        {
            /// <summary>
            /// Return the name of the file
            /// </summary>
            /// <param name="aLogEntry">The LogEntry being logged</param>
            /// <param name="aFormattedLogString">The string being writen to the file</param>
            /// <returns>The name of the file</returns>
            protected internal override string GetFileName(LogEntry aLogEntry, string aFormattedLogString)
            {
                return LogEntryRegexMatchReplacer.Replace(FileNameFormatString, aLogEntry, LoggingErrorHandler);
            }

            /// <summary>
            /// This class does nothing with this method
            /// </summary>
            /// <param name="aLogEntry">The LogEntry being logged</param>
            /// <param name="aFormattedLogString">The string being written to the log</param>
            /// <returns></returns>
            protected internal override string GetIncrementalName(LogEntry aLogEntry, string aFormattedLogString)
            {
                // this is not used in this class
                return "";
            }

            /// <summary>
            /// Validate the format of the FileNameFormatString
            /// </summary>
            /// <param name="aFileNameFormatString">The file name format string to validate</param>
            protected override void ValidateFormatString(string aFileNameFormatString)
            {
                // do not validate--user may format or not, as they wish
            }

            #region Initialization

            /// <summary>
            /// Instantiate a RollOverFormatStrategy
            /// </summary>
            /// <param name="aFullPathFormatString">The format string of the name of the file.</param>
            public RollOverFormatStrategy(string aFullPathFormatString) : base(aFullPathFormatString)
            {
            }

            #endregion
        }

        /// <summary>
        /// RollOverDateStrategy provides date-based roll-over functionality.
        /// </summary>
        public class RollOverDateStrategy : RollOverFormatStrategy
        {
            /// <summary>
            /// Validate the format of the FileNameFormatString
            /// </summary>
            /// <param name="aFileNameFormatString">The file name format string to validate</param>
            protected override void ValidateFormatString(string aFileNameFormatString)
            {
                if (!LogEntryRegexMatchReplacer.IsMatch(aFileNameFormatString, LogEntryRegexMatchReplacer.EVariable.Timestamp))
                    throw new FormatException("The file name must contain a \"timestamp\" format item similar to: {timestamp:yyyyMMdd}");
            }

            #region Initialization

            /// <summary>
            /// Create a new RollOverDateStrategy
            /// </summary>
            /// <remarks>
            /// A FormatException will be thrown if the provided string does not include a format item something like "{timestamp:yyyyMMdd}"
            /// </remarks>
            /// <param name="aFullPathFormatString">The Full path of the log file with a format item like ({timestamp:yyyyMMdd}) for the variable date, for example: "\mylogs\logfile{timestamp:yyyyMMdd}.log"</param>
            public RollOverDateStrategy(string aFullPathFormatString) : base(aFullPathFormatString)
            {
            }

            #endregion
        }


		/// <summary>
		/// RollOverSizeStrategy provides log file size-based roll-over functionality
		/// </summary>
        public class RollOverSizeStrategy : RollOverStrategy
        {
            private readonly long MaxSize;
            private int fileNumber = 0;

            #region properties

            private int FileNumber
            {
                get
                {
                    return fileNumber != 0
                        ? fileNumber
                        : fileNumber = GetFileNumber();
                }
                set { fileNumber = value; }
            }

            #endregion

            #region Initialization

			/// <summary>
			/// Create a new RollOverSizeStrategy
			/// </summary>
			/// <remarks>
			/// A FormatException will be thrown if the provided string does not include the format item "{0}"
			/// </remarks>
			/// <param name="aFullPathFormatString">The Full path of the log file with a format item ({0}) for a variable number (1, 2, 3, etc.), for example: "\mylogs\logfile{0}.log"</param>
			/// <param name="aMaxSize">The maximum size the file should be before it rolls over to the next file</param>
            public RollOverSizeStrategy(string aFullPathFormatString, long aMaxSize) : base(aFullPathFormatString)
            {
                MaxSize = aMaxSize;
            }

            #endregion

			#region Utility

			/// <summary>
			/// Determine the file number to use--essentially find the highest existing file number, otherwise use 1
			/// </summary>
			/// <returns>An int representing the file number to use</returns>
            protected virtual int GetFileNumber() 
			{
				string fileFormat = Path.GetFileName(FileNameFormatString);
				string directory = Path.GetDirectoryName(FileNameFormatString);
                // support for relative paths
                DirectoryInfo di = new DirectoryInfo(directory != "" ? directory : AppDomain.CurrentDomain.BaseDirectory);

				int number = 1;

				if (di.Exists) 
				{
					FileInfo[] fileInfos = di.GetFiles(string.Format(fileFormat, "*"));

					string regPattern = "^" + fileFormat.Replace("{0}", @"([\d]+)") + "$";

					Regex regex = new Regex(regPattern, RegexOptions.IgnoreCase);

					foreach (FileInfo fileInfo in fileInfos)
					{
						Match m = regex.Match(fileInfo.Name);
						if (m.Success)
                            number = Math.Max(number, int.Parse(m.Groups[1].Value));
					}
				}

                return number;
			}

			#endregion

            /// <summary>
            /// Return the variable portion of the name of the log file--a file number
            /// </summary>
            /// <param name="aLogEntry">The LogEntry being logged</param>
            /// <param name="aFormattedLogString">The string being written to the log</param>
            /// <returns>The file number</returns>
            protected internal override string GetIncrementalName(LogEntry aLogEntry, string aFormattedLogString)
            {
                FileInfo fileInfo = new FileInfo(string.Format(FileNameFormatString, FileNumber));
                if ((!fileInfo.Exists) && (FileNumber != 1))
                {
                    FileNumber = GetFileNumber(); // the log files may have been deleted, so reset file number
                    fileInfo = new FileInfo(string.Format(FileNameFormatString, FileNumber));
                }

                // if the file is too big, increment the file number
                if ((fileInfo.Exists) && (fileInfo.Length > MaxSize - aFormattedLogString.Length))
                    FileNumber++;

                return FileNumber.ToString();
            }
        }

		#endregion
    }

}
