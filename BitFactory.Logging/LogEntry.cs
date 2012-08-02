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
	/// Represents an entry that gets logged by a Logger.
	/// </summary>
	/// <remarks>
	/// This class is instantiated by other members of it's assembly.
	/// </remarks>
	[Serializable]
	public class LogEntry
	{
		/// <summary>
		/// The main text of the LogEntry.
		/// </summary>
		private String _message;
		/// <summary>
		/// The timestamp when the LogEntry was created.
		/// </summary>
		private DateTime _date;
		/// <summary>
		/// An optional String that represents the application that generated the LogEntry.
		/// </summary>
		private String _application;
		/// <summary>
		/// The severity of the LogEntry.
		/// </summary>
		private LogSeverity _severity;
		/// <summary>
		/// An optional "category" that can be used to further classify a LogEntry.
		/// Usually this wouls be a String, but can be any Object.
		/// </summary>
		private Object _category;

		/// <summary>
		/// Create a new instance of LogEntry.
		/// </summary>
		protected internal LogEntry() : base()
		{
		}
		/// <summary>
		/// Create a new instance of LogEntry.
		/// </summary>
		/// <param name="aSeverity">The severity of the LogEntry.</param>
		/// <param name="anApplication">A String representing the application.</param>
		/// <param name="aCategory">A category to classify the LogEntry.</param>
		/// <param name="aMessage">The primary text of the LogEntry.</param>
		protected internal LogEntry(LogSeverity aSeverity, String anApplication, Object aCategory, String aMessage) : this( aSeverity, anApplication, aCategory, aMessage, DateTime.Now)
		{
		}
		/// <summary>
		/// Create a new instance of LogEntry.
		/// </summary>
		/// <param name="aSeverity">The severity of the LogEntry.</param>
		/// <param name="anApplication">A String representing the application.</param>
		/// <param name="aCategory">A category to classify the LogEntry.</param>
		/// <param name="aMessage">The primary text of the LogEntry.</param>
		/// <param name="aDate">The timestamp when the LogEntry was created.</param>
		protected internal LogEntry(LogSeverity aSeverity, String anApplication, Object aCategory, String aMessage, DateTime aDate) : this()
		{
			Severity = aSeverity;
			Application = anApplication;
			Category = aCategory;
			Message = aMessage;
			Date = aDate;
		}

		/// <summary>
		/// Gets and sets the application.
		/// </summary>
		public String Application 
		{
			get { return _application; }
			set { _application = value; }
		}
		/// <summary>
		/// Gets and sets the category.
		/// </summary>
		public Object Category 
		{
			get { return _category; }
			set { _category = value; }
		}
		/// <summary>
		/// Gets and sets the timestamp.
		/// </summary>
		public DateTime Date 
		{
			get { return _date; }
			set { _date = value; }
		}
		/// <summary>
		/// Gets and sets the message.
		/// </summary>
		public String Message 
		{
			get { return _message; }
			set { _message = value; }
		}
		/// <summary>
		/// Gets and sets the severity.
		/// </summary>
		public LogSeverity Severity 
		{
			get { return _severity; }
			set { _severity = value; }
		}

		/// <summary>
		/// Gets a friendly String that represents the severity.
		/// </summary>
		public String SeverityString
		{
			get { return Severity.ToString("G"); }
		}

		/// <summary>
		/// Represent the LogEntry as a String.
		/// </summary>
		/// <returns>The String representation of the LogEntry.</returns>
		public override String ToString() 
		{
			return "" + Application
				+ "-" + Category
				+ "-" + SeverityString
				+ "-" + Date
				+ "-" + Message;
		}
	}
}
