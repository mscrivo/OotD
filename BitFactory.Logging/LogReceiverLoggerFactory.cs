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
	/// Instances of this class are used to create
	/// instances of LogReceiverLogger subclasses when
	/// socket connections are made to clients sending LogEntry information.
	/// </summary>
	public class LogReceiverLoggerFactory : LogSocketReaderReceiver
	{
		/// <summary>
		/// The Type of the Logger this instance should create.
		/// </summary>
		private Type loggerType;
		/// <summary>
		/// Any information that needs to be passed to newly
		/// created Loggers.
		/// </summary>
		private Object tagInfo;
		/// <summary>
		/// The Listener that listens for new socket connections.
		/// </summary>
		private LogSocketReader.Listener listener;
		/// <summary>
		/// Create a new instance of LogReceiverLoggerFactory.
		/// </summary>
		/// <param name="aLoggerType">The Type of Logger this instance should eventually create.</param>
		/// <param name="portNumber">The port number on which to listen for new connections.</param>
		/// <param name="aTagInfo">Any information newly created Loggers should be passed.</param>
		public LogReceiverLoggerFactory(Type aLoggerType, int portNumber, Object aTagInfo) 
		{
			CheckLoggerType(aLoggerType);
			loggerType = aLoggerType;
			tagInfo = aTagInfo;
			listener = LogSocketReader.StartListening(portNumber, this);
		}
		/// <summary>
		/// Make sure that aLoggerType is a subclass of LogReceiverLogger.
		/// Throw an InvalidReceiverLoggerException if not.
		/// </summary>
		/// <param name="aLoggerType">The Type to check.</param>
		private void CheckLoggerType(Type aLoggerType) 
		{
			if (!aLoggerType.IsSubclassOf(typeof(LogReceiverLogger)))
				throw new InvalidReceiverLoggerException(aLoggerType);
		}
		/// <summary>
		/// Stop listening.
		/// </summary>
		public void StopListening() 
		{
			listener.Stop();
		}
		/// <summary>
		/// This is a notification that the server socket has been closed.
		/// </summary>
		public void ListenerClosed() 
		{
		}
		/// <summary>
		/// This is a notification that a socket connection has been made
		/// and that a LogSocketReader is ready for reading.
		/// </summary>
		/// <param name="aLogSocketReader">The LogSocketReader that will read from the socket.</param>
		/// <returns>Always returns true, signifying that it is okay to accept more client connections.</returns>
		public bool ReaderReady(LogSocketReader aLogSocketReader) 
		{
			aLogSocketReader.Logger = CreateNewLogger(aLogSocketReader);
			aLogSocketReader.Start();
			return true;
		}
		/// <summary>
		/// Create and initialize a new Logger.
		/// </summary>
		/// <param name="aLogSocketReader">The LogSocketReader.</param>
		/// <returns>The newly created Logger</returns>
		private Logger CreateNewLogger(LogSocketReader aLogSocketReader) 
		{
			/*
				 * Instead of calling the follwing constructor,
				 * make it easier on developers who subclass LogReceiverLogger
				 * by setting properties after creation instead.
				 * return (Logger) loggerType.GetConstructor( new Type[2] { typeof(LogSocketReader), typeof(Object) } ).Invoke( new Object[2] { aLogSocketReader, tagInfo } );
				 */
			LogReceiverLogger aLogger = (LogReceiverLogger) loggerType.GetConstructor(new Type[0]).Invoke(new Object[0]);
			aLogger.LogSocketReader = aLogSocketReader;
			aLogger.TagValue = tagInfo;
			aLogger.PreStart(this);
			return aLogger;
		}
		/// <summary>
		/// An Exception class thrown when an invalid Logger is used with LogReceiverLoggerFactory.
		/// </summary>
		public class InvalidReceiverLoggerException : Exception 
		{
			/// <summary>
			/// Create a new instance of InvalidReceiverLoggerException.
			/// </summary>
			/// <param name="aType">The invalid Type of Logger attempted to be used.</param>
			public InvalidReceiverLoggerException(Type aType)
				: base(aType.ToString() + " must be a subclass of LogReceiverLogger.") 
			{
			}
		}
	}
}
