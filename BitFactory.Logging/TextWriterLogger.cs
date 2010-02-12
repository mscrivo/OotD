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
using System.IO;

namespace BitFactory.Logging
{
	/// <summary>
	/// Instances of TextWriterLogger write LogEntry information to a TextWriter.
	/// </summary>
	/// <remarks>
	/// It's most likely use will be as a System console logger--instances of which
	/// can be created by using the static method NewConsoleLogger().
	/// </remarks>
	public class TextWriterLogger : Logger
	{
		/// <summary>
		/// The TextWriter to which LogEntries are written.
		/// </summary>
		private TextWriter _textWriter;
		/// <summary>
		/// Gets and sets the TestWriter.
		/// </summary>
		protected TextWriter TextWriter 
		{
			get { return _textWriter; }
			set { _textWriter = value; }
		}
		/// <summary>
		/// Write the String representatin of a LogEntry to the TextWriter.
		/// </summary>
		/// <param name="s">The String representation of a LogEntry.</param>
		/// <returns>true upon success, false upon failure.</returns>
		protected override bool WriteToLog(String s) 
		{
			try 
			{
				TextWriter.WriteLine(s);
				return true;
			} 
			catch(Exception ex)
			{
                OnLoggingError(this, "Error writing to log", ex);
				return false;
			}
		}
		/// <summary>
		/// Create a new instance of TextWriterLogger.
		/// </summary>
		private TextWriterLogger() : base()
		{
		}
		/// <summary>
		/// Create a new instance of TextWriterLogger.
		/// </summary>
		/// <param name="aTextWriter">A TextWriter to write LogEntries.</param>
		public TextWriterLogger(TextWriter aTextWriter) : this() 
		{
			TextWriter = aTextWriter;
		}
		/// <summary>
		/// Utility for creating a logger that writes to the System.Console.
		/// </summary>
		/// <returns>A new TextWriterLogger.</returns>
		public static TextWriterLogger NewConsoleLogger() 
		{
			return new TextWriterLogger(Console.Out);
		}
	}
}
