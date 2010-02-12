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
	/// Implementers of this interface are sent the ReaderReady message when a client logger
	/// has made a connection to LogSocketReader.Listener
	/// </summary>
	/// <seealso cref="LogSocketReader"/>
	public interface LogSocketReaderReceiver
	{
		/// <summary>
		/// A notification that the listener has stopped listening.
		/// </summary>
		void ListenerClosed();

		/// <summary>
		/// When this method is sent, a connection has been made and aLogSocketReader
		/// is ready to start reading LogEntries.
		/// Return true if additional connections can be made to the port, or false if the
		/// current connection is the only one allowed.
		/// It is up to the receiver of this message (or some other object to which it delegates)
		/// to send the Start() message to aLogSocketReader.
		/// </summary>
		bool ReaderReady(LogSocketReader aLogSocketReader);
	}
}
