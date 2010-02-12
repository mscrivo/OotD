/* 
 * (C) Copyright 2002, 2009 - Lorne Brinkman - All Rights Reserved.
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
	/// Concrete subclasses of this abstract class are responsible for formatting LogEntries.
	/// </summary>
	/// <remarks>
	/// Subclasses should override the asString(LogEntry) method.
	/// Also, users can freely modify the formatString attribute for timestamp formatting.
	/// </remarks>
	public abstract class LogEntryFormatter
	{
        /// <summary>
        /// A deleate used to trigger the LoggingError event in a logger
        /// </summary>
        protected Logger.LoggingErrorHandler LoggingErrorHandler { get; set; }

		/// <summary>
		/// A format String for DateTime.
		/// </summary>
		private String _formatString = "";
		/// <summary>
		/// Gets and sets the format String.
		/// </summary>
		public String FormatString
		{
			get { return _formatString; }
			set { _formatString = value; }
		}

		/// <summary>
		/// String format of the LogEntry.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry to format.</param>
		/// <returns>A nicely formatted String.</returns>
		protected internal abstract String AsString(LogEntry aLogEntry);

		/// <summary>
		/// String format of the DateTime of the LogEntry.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry whose timestamp needs formatting.</param>
		/// <returns>A nicely formatted String.</returns>
		protected String DateString(LogEntry aLogEntry) 
		{
			return  aLogEntry.Date.ToString(FormatString);
		}
		/// <summary>
		/// LogEntryFormatter constructor.
		/// </summary>
		protected LogEntryFormatter(Logger.LoggingErrorHandler aLoggingErrorHandler) : base()
		{
            LoggingErrorHandler = aLoggingErrorHandler;
		}
	}
}
