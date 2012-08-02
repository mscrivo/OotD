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
	/// Instances store LogEntries in memory.
	/// </summary>
	public class MemoryLogger : Logger
	{
		/// <summary>
		/// maximum number of LogEntries to store.
		/// </summary>
		private int _capacity = 50;
		/// <summary>
		/// A list of LogEntries.
		/// </summary>
		private IList _logEntries = new ArrayList();

		/// <summary>
		/// Gets and sets the capacity.
		/// </summary>
		public int Capacity 
		{
			get { return _capacity; }
			set { _capacity = value; }
		}
		/// <summary>
		/// Gets and sets the list of logEntries.
		/// </summary>
		protected IList LogEntries 
		{
			get { return _logEntries; }
			set { _logEntries = value; }
		}

		/// <summary>
		/// Create an instance of MemoryLogger.
		/// </summary>
		public MemoryLogger() : base()
		{
		}
		/// <summary>
		/// Create an instance of MemoryLogger.
		/// </summary>
		/// <param name="aCapacity">The capacity of the Logger.</param>
		public MemoryLogger(int aCapacity) : this() 
		{
			Capacity = aCapacity;
		}
		/// <summary>
		///  Copy constructor.
		/// </summary>
		/// <param name="aMemoryLogger">The MemoryLogger to copy.</param>
		public MemoryLogger(MemoryLogger aMemoryLogger) : this(aMemoryLogger.Capacity)
		{
			LogEntries = (ArrayList)((ArrayList)aMemoryLogger.LogEntries).Clone();
		}

		/// <summary>
		/// Clear all the LogEntries
		/// </summary>
		public void Clear() 
		{
			lock(this) { LogEntries.Clear(); }
		}
		/// <summary>
		/// A convenience method that copies a MemoryLogger.
		/// </summary>
		/// <returns>A new MemoryLogger with the same LogEntries as in the receiver.</returns>
		public MemoryLogger Copy() 
		{
			return new MemoryLogger(this);
		}

		/// <summary>
		/// Store the LogEntry.
		/// If my capacity has been reached, remove the oldest entry.
		/// Add the new entry to the collection.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry to log.</param>
		/// <returns>Always return true.</returns>
		protected internal override bool DoLog(LogEntry aLogEntry) 
		{
			lock(this) 
			{
				if ( Count == Capacity )
					LogEntries.RemoveAt(0);
				LogEntries.Add( aLogEntry );
			}
			return true;
		}
		/// <summary>
		/// Create and return an Array of LogEntry.
		/// </summary>
		/// <returns>A LogEntry Array.</returns>
		public LogEntry[] GetLog() 
		{
			return (LogEntry[])((ArrayList)LogEntries).ToArray(typeof(LogEntry));
		}

		/// <summary>
		/// Send all LogEntries to another Logger.
		/// </summary>
		/// <param name="aLogger">The Logger to which the receiver should send all its LogEntries.</param>
		public void LogAllTo(Logger aLogger) 
		{
			lock(this) 
			{
                foreach (LogEntry logEntry in LogEntries)
                    aLogger.Log(logEntry);
			}
		}
		
		/// <summary>
		/// Get the number of LogEntries currently held by the Logger.
		/// </summary>
		public int Count
		{
			get { return LogEntries.Count; }
		}
		/// <summary>
		/// Transfer all held LogEntries to another Logger.
		/// This differs from LogAllTo in that successfully logged entries
		/// are removed from the receiver. Also, a DoLog message is sent, as opposed to Log.
		/// Also, the receiver stops trying to log once it gets a failure.
		/// </summary>
		/// <param name="aLogger">The Logger to which LogEntries should be transfered.</param>
		/// <returns>true if everything logged successfully, false otherwise.</returns>
		private bool MoveTo(Logger aLogger) 
		{
			// this method should only be called from a thread-safe method

			ArrayList aList = (ArrayList)((ArrayList)LogEntries).Clone();
            foreach (LogEntry logEntry in aList)
                if (aLogger.DoLog(logEntry))
                    LogEntries.RemoveAt(0);
                else
                    return false;
			return true;
		}
		/// <summary>
		/// Transfer all held LogEntries to another Logger.
		/// This differs from LogAllTo in that successfully logged entries
		/// are removed from the receiver. Also, a DoLog message is sent, as opposed to Log.
		/// Also, the receiver stops trying to log once it gets a failure.
		/// </summary>
		/// <param name="aLogger">The Logger to which LogEntries should be transfered.</param>
		/// <returns>true if everything logged successfully, false otherwise.</returns>
		public bool TransferTo(Logger aLogger) 
		{
			lock(this) 
			{
				while (Count > 0)	// this Count check is redundant with the carefully placed locks
					if (!MoveTo(aLogger))
						return false;

				return true;
			}
		}

	}
}
