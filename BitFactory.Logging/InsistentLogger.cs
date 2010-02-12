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
using System.Threading;

namespace BitFactory.Logging
{
	/// <summary>
	/// Wrap instances of this class around any other logger. If the wrapped logger fails
	/// to successfully log an entry, then this instance will keep trying to log the entry.
	/// </summary>
	/// <remarks>
	/// example: Logger myLogger = new SerialSocketLogger("localhost", 12345);
	///          /* set any desired filters or formatters for myLogger
	///             then wrap in an InsistentLogger... */
	///          myLogger = new InsistentLogger(myLogger,200,60);
	///          /* now myLogger can be used like any other logger, but with more
	///             reliability in entries reaching their destination */
	/// </remarks>
	public class InsistentLogger : Logger
	{
		/// <summary>
		/// The Logger being wrapped.
		/// </summary>
		private Logger _logger;
		/// <summary>
		/// A MemoryLogger to store LogEntries.
		/// </summary>
		private MemoryLogger _memoryLogger;
		/// <summary>
		/// The number of seconds between retries at logging.
		/// </summary>
		private int _intervalSeconds;
		/// <summary>
		/// A Thread.
		/// </summary>
		private Thread _thread;

		/// <summary>
		/// Gets and sets the Logger.
		/// </summary>
		private Logger Logger 
		{
			get { return _logger; }
			set { _logger = value; }
		}
		/// <summary>
		/// Gets and sets the MemoryLogger.
		/// </summary>
		private MemoryLogger MemoryLogger 
		{
			get { return _memoryLogger; }
			set { _memoryLogger = value; }
		}
		/// <summary>
		/// Gets and sets the interval seconds.
		/// </summary>
		private int IntervalSeconds 
		{
			get { return _intervalSeconds; }
			set { _intervalSeconds = value; }
		}
		/// <summary>
		/// Gets and sets the Thread.
		/// </summary>
		private Thread Thread 
		{
			get { return _thread; }
			set { _thread = value; }
		}
		/// <summary>
		/// Gets the active Logger currently being logged to.
		/// </summary>
		private Logger ActiveLogger 
		{
			get { return Thread == null ? Logger : MemoryLogger; }
		}

		/// <summary>
		/// Create a new instance of InsistentLogger.
		/// </summary>
		protected InsistentLogger() : base()
		{
		}
		/// <summary>
		/// Create a new instance of InsistentLogger.
		/// </summary>
		/// <param name="aLogger">The actual Logger that should eventually receive LogEntries.</param>
		/// <param name="capacity">The maximum number of LogEntries that should be stored for retrying.</param>
		/// <param name="anIntervalSeconds">The number of seconds between each retry at logging.</param>
		public InsistentLogger(Logger aLogger, int capacity, int anIntervalSeconds) : this()
		{
			Logger = aLogger;
			MemoryLogger = new MemoryLogger(capacity);
			IntervalSeconds = anIntervalSeconds;
		}

		/// <summary>
		/// Send aLogEntry to the ActiveLogger, either the desired Logger, or the MemoryLogger.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry being logged</param>
		/// <returns>Always returns true.</returns>
		protected internal override bool DoLog(LogEntry aLogEntry) 
		{
			lock(this) { return ActiveLogger.DoLog(aLogEntry) || HandleLogFailure(aLogEntry); }
		}

		/// <summary>
		/// Send the LogEntry to the MemoryLogger, then start the retrying thread.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry to log.</param>
		/// <returns>Always returns true.</returns>
		private bool HandleLogFailure(LogEntry aLogEntry) 
		{
			MemoryLogger.DoLog(aLogEntry);
			StartThread();
			return true;
		}
		/// <summary>
		/// Start the retrying thread.
		/// </summary>
		public void StartThread() 
		{
			Thread = new Thread(new ThreadStart(TryLogging));
			Thread.IsBackground = true;
			Thread.Start();
		}
		/// <summary>
		/// Stop the retrying thread.
		/// </summary>
		public void StopThread() 
		{
			Thread aThread = Thread;
			Thread = null;
			aThread.Interrupt();
		}
		/// <summary>
		/// The logging thread's main loop.
		/// Attempt to send the LogEntries in the MemoryLogger to the desired Logger.
		/// </summary>
		private void TryLogging() 
		{
			do 
			{
				try 
				{
					Thread.Sleep(IntervalSeconds*1000);
				} 
				catch (ThreadInterruptedException) 
				{
				}
			} while ( (Thread != null) && !Transfer() );

			Thread = null;
		}
		/// <summary>
		/// Attempt to transfer the stored LogEntries to their proper destination.
		/// </summary>
		/// <returns>true if all LogEntries transfered, false otherwise.</returns>
		private bool Transfer() 
		{
			lock(this) { return MemoryLogger.TransferTo(Logger); }
		}
	}
}
