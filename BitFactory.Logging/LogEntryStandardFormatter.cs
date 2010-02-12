/* 
 * (C) Copyright 2002 - Lorne Brinkman - All Rights Reserved.
 * http://www.TheObjectGuy.com
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 * 
 *  - Redistributions of source code must retain the above copyright notice,
 *    this list of conditions and the following disclaimer.
 * 
 *  - Redistributions in binary form must reproduce the above copyright notice,
 *    this list of conditions and the following disclaimer in the documentation
 *    and/or other materials provided with the distribution.
 * 
 *  - Neither the name "Lorne Brinkman", "The Object Guy", nor the name "Bit Factory"
 *    may be used to endorse or promote products derived from this software without
 *    specific prior written permission.
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
	/// This formatter is the default one that formats all LogEntry information in a reasonable way.
	/// </summary>
	public class LogEntryStandardFormatter : LogEntryFormatter
	{
		/// <summary>
		/// Create a reasonably formatted String that contains all the LogEntry information.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry to format.</param>
		/// <returns>A nicely formatted String.</returns>
		protected internal override String AsString(LogEntry aLogEntry) 
		{
			String appString = "";
			if ( aLogEntry.Application != null )
				appString = "[" + aLogEntry.Application + "] -- ";
			if ( aLogEntry.Category != null ) 
				try 
				{
					appString = appString + "{" + aLogEntry.Category + "} --";
				} 
				catch 
				{
				}
			return appString + "<" + aLogEntry.SeverityString + "> -- "
				+ DateString(aLogEntry) + " -- " + aLogEntry.Message;
		}

		/// <summary>
		/// Create a new instance of LogEntryStandardFormatter.
		/// </summary>
		public LogEntryStandardFormatter(Logger.LoggingErrorHandler aLoggingErrorHandler) : base(aLoggingErrorHandler)
		{
		}
	}
}
