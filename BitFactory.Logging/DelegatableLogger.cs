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
using System.Text;

namespace BitFactory.Logging
{
	/// <summary>
	/// DelegatableLogger can be used to easily create a logger to which will use a given delegate to actually do the logging.
	/// </summary>
	/// <remarks>
	/// One of two different delegates can be used:
	/// one for handling a complete LogEntry if complicated handling is required,
	/// the other for simply handing a pre-formatted string to be logged
	/// </remarks>
    public class DelegatableLogger : Logger
    {
		/// <summary>
		/// This is a delegate used to handle writing log entries based on the LogEntry
		/// </summary>
        public delegate bool DoLogDelegate(LogEntry aLogEntry);
		/// <summary>
		/// This is a delegate used to handle writing log entries based in the string to be written to the log
		/// </summary>
        public delegate bool WriteToLogDelegate(string aString);

        private readonly DoLogDelegate _DoLog;
        private readonly WriteToLogDelegate _WriteToLog;

        #region Initialization

		/// <summary>
		/// Create a new DelegatableLogger using a DoLogDelegate
		/// </summary>
		/// <param name="aDoLogDelegate">A DoLogDelegate</param>
        public DelegatableLogger(DoLogDelegate aDoLogDelegate) : base()
        {
            _DoLog = aDoLogDelegate;
        }

		/// <summary>
		/// Create a new DelegatableLogger using a WriteToLogDelegate
		/// </summary>
		/// <param name="aWriteToLogDelegate">A WriteToLogDelegate</param>
        public DelegatableLogger(WriteToLogDelegate aWriteToLogDelegate) : base()
        {
            _WriteToLog = aWriteToLogDelegate;
        }

        #endregion

        #region Logging

		/// <summary>
		/// Log a LogEntry
		/// </summary>
		/// <param name="aLogEntry">The LogEntry to be logged</param>
		/// <returns>true if successfully logged, otherwise false</returns>
        protected internal override bool DoLog(LogEntry aLogEntry)
        {
            return _DoLog != null
                ? _DoLog(aLogEntry)
                : base.DoLog(aLogEntry);
        }

		/// <summary>
		/// Write a string to the log
		/// </summary>
		/// <param name="s">The string to write to the log</param>
		/// <returns>true if successfully written to the log, otherwise false</returns>
        protected override bool WriteToLog(string s)
        {
            return _WriteToLog != null
                ? _WriteToLog(s)
                : base.WriteToLog(s);
        }

        #endregion

    }
}
