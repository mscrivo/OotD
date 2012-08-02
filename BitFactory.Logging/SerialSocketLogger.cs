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
using System.Net.Sockets;

namespace BitFactory.Logging
{
	/// <summary>
	/// Instances of SerialSocketLogger write out serialized LogEntries to a Socket
	/// <seealso cref="LogSocketReader"/>
	/// </summary>
	public class SerialSocketLogger : SerialLogger
	{
		/// <summary>
		/// The TcpClient used for writing LogEntries.
		/// </summary>
		private TcpClient _tcpClient;
		/// <summary>
		/// The name of the remote host to which the receiver is logging.
		/// </summary>
		private String _hostName;
		/// <summary>
		/// The port number of the socket.
		/// </summary>
		private int _port;

		/// <summary>
		/// Gets and sets the TcpClient.
		/// </summary>
		public TcpClient TcpClient 
		{
			get { return _tcpClient; }
			set { _tcpClient = value; }
		}
		/// <summary>
		/// Gets and sets the name of the remote host.
		/// </summary>
		public String HostName 
		{
			get { return _hostName; }
			set { _hostName = value; }
		}
		/// <summary>
		/// Gets and sets the port number.
		/// </summary>
		public int Port 
		{
			get { return _port; }
			set { _port = value; }
		}
		/// <summary>
		/// Create a new instance of SerialSocketLogger.
		/// </summary>
		protected SerialSocketLogger() : base()
		{
		}
		/// <summary>
		/// Create a new instance of SerialSocketLogger.
		/// </summary>
		/// <param name="aHostName">The name of the remote host.</param>
		/// <param name="aPort">The port number.</param>
		public SerialSocketLogger(String aHostName, int aPort) : this()
		{
			HostName = aHostName;
			Port = aPort;
			//ResetSocket(); -- do not reset socket in constructor... now the connection will not be made until logging is attempted, which plays better with AsyncLogger, for example.
		}
		/// <summary>
		/// Reset the socket and try one more time
		/// </summary>
		/// <param name="aLogEntry">The LogEntry that failed to get logged.</param>
		/// <param name="ex">The exception thrown during the logging attempt.</param>
		/// <returns>true upon success, false upon failure.</returns>
		protected override bool HandleLogFailure(LogEntry aLogEntry, Exception ex) 
		{
			ResetSocket();
			try 
			{
				TryToLog(aLogEntry);
				return true;
			} 
			catch(Exception e)
			{
                OnLoggingError(this, "Error logging to socket", e);
				return false;
			}
		}
		/// <summary>
		/// Safely try to reset the socket, handling errors.
		/// </summary>
		protected void ResetSocket() 
		{
			try 
			{
				TryToResetSocket();
			} 
			catch
			{
			}
		}
		/// <summary>
		/// Attempt to reset the socket
		/// </summary>
		protected void TryToResetSocket()
		{
			TcpClient = new TcpClient(HostName, Port);
			Out = TcpClient.GetStream();
		}

	}
}
