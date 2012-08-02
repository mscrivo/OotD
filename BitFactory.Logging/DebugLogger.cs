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
using System.Diagnostics;

namespace BitFactory.Logging
{
	/// <summary>
	/// DebugLogger encapsulates logging to System.Diagnostics.Debug in a Logger.
	/// </summary>
	public class DebugLogger : Logger
	{
		/// <summary>
		/// Send aLogEntry information to System.Diagnostics.Debug.
		/// </summary>
		/// <param name="aLogEntry">A LogEntry.</param>
		/// <returns>true upon success, which this Logger always assumes.</returns>
		protected internal override bool DoLog(LogEntry aLogEntry) 
		{
			try 
			{
				Debug.WriteLine(
					Formatter.AsString(aLogEntry),
					(aLogEntry.Category != null ? aLogEntry.Category.ToString() : null));
				return true;
			} 
			catch(Exception ex)
			{
                OnLoggingError(this, "Error in DebugLogger", ex);
				return false;
			}
		}
		/// <summary>
		/// Create a new instance of DebugLogger.
		/// </summary>
		public DebugLogger() : base() 
		{
		}
	}
}
