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
using System.Threading;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Net;

namespace BitFactory.Logging
{
	/// <summary>
	/// An instance of LogSocketReader reads serialized LogEntries from a socket
	/// and subsequently logs the LogEntries to its respective logger.
	/// </summary>
	/// <remarks>
	///	Instances are typically created via the static method StartListening(aPort,aLogSocketReaderReceiver).
	/// </remarks>
	public class LogSocketReader
	{
		/// <summary>
		/// The client Socket.
		/// </summary>
		private Socket _socket;
		/// <summary>
		/// The internal Logger.
		/// </summary>
		private Logger _logger;
		/// <summary>
		/// The Thread used for reading.
		/// </summary>
		private Thread _thread;

		/// <summary>
		/// Gets and sets the socket.
		/// </summary>
		private Socket Socket 
		{
			get { return _socket; }
			set { _socket = value; }
		}
		/// <summary>
		/// Gets and sets the Logger.
		/// </summary>
		public Logger Logger 
		{
			get { return _logger; }
			set { _logger = value; }
		}
		/// <summary>
		/// Gets and sets the Thread
		/// </summary>
		private Thread Thread 
		{
			get { return _thread; }
			set { _thread = value; }
		}
		/// <summary>
		/// Gets a String representation of the remote endpoint address (eg. 1.2.3.4:2350).
		/// </summary>
		public String RemoteAddress 
		{
			get 
			{
				return Socket.Connected ? Socket.RemoteEndPoint.ToString() : "Not connected";
			}
		}
		/// <summary>
		/// Inner class used for listening for connections.
		/// </summary>
		public class Listener 
		{
			/// <summary>
			/// An IP Address
			/// </summary>
			private IPAddress _ipAddress;
			/// <summary>
			/// A port number.
			/// </summary>
			private int _port;
			/// <summary>
			/// A TcpListener.
			/// </summary>
			private TcpListener _tcpListener;
			/// <summary>
			/// A LogSocketReaderReceiver.
			/// </summary>
			private LogSocketReaderReceiver _logSocketReaderReceiver;
			/// <summary>
			/// A Thread.
			/// </summary>
			private Thread _thread;

			/// <summary>
			/// Gets and sets the IP address.
			/// </summary>
			private IPAddress IpAddress
			{
				get { return _ipAddress; }
				set { _ipAddress = value; }
			}
			/// <summary>
			/// Gets and sets the port number.
			/// </summary>
			private int Port 
			{
				get { return _port; }
				set { _port = value; }
			}
			/// <summary>
			/// Gets and sets the TcpListener.
			/// </summary>
			private TcpListener TcpListener 
			{
				get { return _tcpListener; }
				set { _tcpListener = value; }
			}
			/// <summary>
			/// Gets and sets the LogSocketReaderReceiver.
			/// </summary>
			private LogSocketReaderReceiver LogSocketReaderReceiver 
			{
				get { return _logSocketReaderReceiver; }
				set { _logSocketReaderReceiver = value; }
			}
			/// <summary>
			/// Gets and sets the Thread.
			/// </summary>
			protected Thread Thread 
			{
				get { return _thread; }
				set { _thread = value; }
			}
			/// <summary>
			/// Create a new instance of Listener listening on any IP address
			/// </summary>
			/// <remarks>
			/// An exception may be raised if there is a problem
			/// opening the server socket on the specified port.
			/// </remarks>
			/// <param name="aPort">A port number on which to listen</param>
			/// <param name="aReceiver">A LogSocketReaderReceiver</param>
			internal Listener(int aPort, LogSocketReaderReceiver aReceiver) : this(IPAddress.Any, aPort, aReceiver)
			{
			}
			/// <summary>
			/// Create a new instance of Listener.
			/// </summary>
			/// <remarks>
			/// An exception may be raised if there is a problem
			/// opening the server socket on the specified port.
			/// </remarks>
			/// <param name="anIpAddress">The IP address on which a port will be opened to listen</param>
			/// <param name="aPort">A port number on which to listen</param>
			/// <param name="aReceiver">A LogSocketReaderReceiver</param>
			internal Listener(IPAddress anIpAddress, int aPort, LogSocketReaderReceiver aReceiver) 
			{
				Port = aPort;
				IpAddress = anIpAddress;

				// exceptions will be thrown if there is a problem with the port

				TcpListener = new TcpListener(IpAddress, Port);
				TcpListener.Start();

				LogSocketReaderReceiver = aReceiver;
				Thread = new Thread(new ThreadStart(Run));
				Thread.IsBackground = true;
				Thread.Start();
			}
			/// <summary>
			/// Get the reader.
			/// </summary>
			/// <returns>A LogSocketReader.</returns>
			private LogSocketReader GetReader() 
			{
				try 
				{
					return new LogSocketReader(TcpListener.AcceptSocket());
				} 
				catch
				{
					return null;
				}
			}
			/// <summary>
			/// The loop for the internal Thread that listens for connections.
			/// </summary>
			private void Run() 
			{
				LogSocketReader reader;
				do 
				{
					reader = GetReader();
				} while ((reader != null) && LogSocketReaderReceiver.ReaderReady(reader));
				LogSocketReaderReceiver.ListenerClosed();		
			}
			/// <summary>
			/// Stop listening on the port.
			/// </summary>
			public void Stop() 
			{
				try 
				{
					TcpListener.Stop();
				} 
				catch
				{
				}
			}

		}
		/// <summary>
		/// Create a new instance of LogSocketReader.
		/// The best way to create instances of this class is to use the static method StartListening.
		/// </summary>
		private LogSocketReader() : base()
		{
		}
		/// <summary>
		/// Create a new instance of LogSocketReader.
		/// The best way to create instances of this class is to use the static method StartListening.
		/// </summary>
		/// <param name="aSocket">A Socket from which to read LogEntries</param>
		public LogSocketReader(Socket aSocket) : this()
		{
			Socket = aSocket;
			Thread = new Thread( new ThreadStart(Run));
			Thread.IsBackground = true;
		}
		/// <summary>
		/// Create a new instance of LogSocketReader.
		/// The best way to create instances of this class is to use the static method StartListening.
		/// </summary>
		/// <param name="aSocket">A Socket from which to read LogEntries</param>
		/// <param name="aLogger">A logger to which received LogEntries are logged.</param>
		public LogSocketReader(Socket aSocket, Logger aLogger) : this(aSocket)
		{
			Logger = aLogger;
		}
		/// <summary>
		/// Log a LogEntry.
		/// </summary>
		/// <param name="aLogEntry">The LogEntry to process.</param>
		protected void ProcessLogEntry(LogEntry aLogEntry) 
		{
			Logger.Log(aLogEntry);
		}
		/// <summary>
		/// A notification that the reader has been closed.
		/// Log status information.
		/// </summary>
		protected void ReaderClosed() 
		{
			Logger.LogStatus( "LogSocketReader", "connection closed");
		}
		/// <summary>
		/// The main loop of the Thread.
		/// Read LogEntries from the Socket Stream.
		/// </summary>
		private void Run() 
		{
			BinaryFormatter formatter = new BinaryFormatter();
			try 
			{
				Stream inStream = new NetworkStream(Socket);
				Object objectFromStream = null;
				try 
				{
					while (true) 
					{
						objectFromStream = formatter.Deserialize(inStream);
						ProcessLogEntry( (LogEntry) objectFromStream);
					}
				} 
				catch (FileLoadException) 
				{
					HandleIncompatibleLogEntry(objectFromStream);
				}
				catch (InvalidCastException) 
				{
					HandleIncompatibleLogEntry(objectFromStream);
				}
				catch (System.Runtime.Serialization.SerializationException)
				{
				}
				catch (Exception e)
				{
					Logger.LogError("LogSocketReader", e);
				}
				Socket.Close();
				ReaderClosed();	
			} 
			catch (Exception ex) 
			{
				Logger.LogError("LogSocketReader", ex);
			}

		}
		/// <summary>
		/// Handle exceptions due to LogEntry incompatibilities.
		/// </summary>
		/// <param name="objectFromStream">The Object read from the stream.</param>
		private void HandleIncompatibleLogEntry(Object objectFromStream) 
		{
			Logger.LogError("LogSocketReader", "Objects in the socket stream are not compatible with my version of LogEntry");
		}
		/// <summary>
		/// Create a new Listener and start listening on the port.
		/// </summary>
		/// <remarks>
		/// An exception may be raised if there is a problem
		/// opening the server socket on the specified port.
		/// </remarks>
		/// <param name="aPort">The port on which to listen.</param>
		/// <param name="aReceiver">A LogSocketReaderReceiver that will be notified of new connections.</param>
		/// <returns>A Listener.</returns>
		public static Listener StartListening(int aPort, LogSocketReaderReceiver aReceiver) 
		{
			return new Listener( aPort, aReceiver);
		}
		/// <summary>
		/// Create a new Listener and start listening on the port.
		/// </summary>
		/// <remarks>
		/// An exception may be raised if there is a problem
		/// opening the server socket on the specified port.
		/// </remarks>
		/// <param name="anIpAddress">The IP address on which to listen.</param>
		/// <param name="aPort">The port on which to listen.</param>
		/// <param name="aReceiver">A LogSocketReaderReceiver that will be notified of new connections.</param>
		/// <returns>A Listener.</returns>
		public static Listener StartListening(IPAddress anIpAddress, int aPort, LogSocketReaderReceiver aReceiver) 
		{
			return new Listener( anIpAddress, aPort, aReceiver);
		}

		/// <summary>
		/// Start the Thread.
		/// </summary>
		public void Start() 
		{
			Thread.Start();
		}
		/// <summary>
		/// Stop the Thread.
		/// </summary>
		public void Stop() 
		{
			try 
			{
				Socket.Close();
			} 
			catch
			{
			}
		}
	}
}
