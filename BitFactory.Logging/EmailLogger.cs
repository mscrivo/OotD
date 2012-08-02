/* 
 * (C) Copyright 2002 - 2009 - Lorne Brinkman - All Rights Reserved.
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
using System.Net.Mail;

namespace BitFactory.Logging
{
	/// <summary>
	/// An EmailLogger sends log information via an email message.
	/// </summary>
	/// <remarks>
	/// If the subject attribute is not explicitly set, then it will automatically be filled
	/// with the logEntry's application, category, and severity.
	/// </remarks>
	public class EmailLogger : Logger
	{
		/// <summary>
		/// The "from" for the email.
		/// </summary>
		private String _from;
		/// <summary>
		/// The "to" for the email.
		/// </summary>
		private String _to;
		/// <summary>
		/// The "subject" of the email. This can be a format string.
		/// </summary>
		private String _subject;
        /// <summary>
        /// The SMTP client
        /// </summary>
        private SmtpClient _smtpClient;

		/// <summary>
		/// Gets and sets the "from" for the email.
		/// </summary>
		public String From 
		{
			get { return _from; }
			set { _from = value; }
		}
		/// <summary>
		/// Gets and sets the "to" for the email.
		/// </summary>
		public String To 
		{
			get { return _to; }
			set { _to = value; }
		}
		/// <summary>
		/// Gets and sets the "subject" of the email.
        /// The subject can be a format string containing the same parameters the LogEntryFormatStringFormatter class uses.
        /// For example, "{timestamp:G}..{message}"
		/// </summary>
		public String Subject 
		{
			get { return _subject; }
			set { _subject = value; }
		}
        /// <summary>
        /// Gets and sets the SmtpClient
        /// </summary>
        public SmtpClient SmtpClient
        {
            get { return _smtpClient; }
            set { _smtpClient = value; }
        }

		/// <summary>
		/// Create an instance of EmailLogger.
		/// </summary>
        /// <param name="anSmtpClient">The SmtpClient object the logger will use to send emails.</param>
		/// <param name="aFrom">The "from" for the emails that get sent.</param>
		/// <param name="aTo">The "to" for the emails that get sent.</param>
        public EmailLogger(SmtpClient anSmtpClient, String aFrom, String aTo) : this(anSmtpClient, aFrom, aTo, null)
		{			
		}
		/// <summary>
		/// Create an instance of EmailLogger.
		/// </summary>
        /// <param name="anSmtpClient">The SmtpClient object the logger will use to send emails.</param>
        /// <param name="aFrom">The "from" for the emails that get sent.</param>
		/// <param name="aTo">The "to" for the emails that get sent.</param>
		/// <param name="aSubject">The "subject" of the emails that get sent. This can be a format string.</param>
		public EmailLogger(SmtpClient anSmtpClient, String aFrom, String aTo, String aSubject) : base()
		{
            SmtpClient = anSmtpClient;
			From = aFrom;
			To = aTo;
			Subject = aSubject;
		}

		/// <summary>
		/// Send the email representing aLogEntry.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry.</param>
		/// <returns>true upon success, false upon failure.</returns>
		protected internal override bool DoLog(LogEntry aLogEntry) 
		{
			try 
			{
                var formatString = string.IsNullOrEmpty(Subject)
                    ? "[{application}]" + (((aLogEntry.Category != null) && (aLogEntry.Category.ToString() != "")) ? " -- [{category}]" : "" ) + " -- [{severity}]"
                    : Subject;
                var fomattedSubject = LogEntryRegexMatchReplacer.Replace(formatString, aLogEntry, OnLoggingError);

				SmtpClient.Send( From, To, fomattedSubject, Formatter.AsString( aLogEntry));

				return true;
			} 
			catch(Exception ex)
			{
                OnLoggingError(this, "Error sending email", ex);
				return false;
			}
		}
	}
}
