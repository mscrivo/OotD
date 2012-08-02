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
	/// This class can be used as a convenient base class when creating
	/// a logger that receives LogEntries over a Socket.
	/// </summary>
	/// <remarks>
	/// Keep in mind that the logging methods that are called on instances of this class are
	/// done so in a different thread than the main thread of the application.
	/// Depending on the action taken, this may impact the subclasses' impementation
	/// of those mthods. e.g. Writing to the GUI from a different thread often
	/// requires using the various "Invoke" methods of the Control class.
	/// </remarks>
	public abstract class LogReceiverLogger : Logger 
	{
		/// <summary>
		/// This is a placeholder for any Object a newly created
		/// instance of this class needs access.
		/// </summary>
		private Object _tagValue;
		/// <summary>
		/// The LogSocketReader that is being used to
		/// read LogEntries from the Socket.
		/// </summary>
		private LogSocketReader _logSocketReader;
		/// <summary>
		/// Gets and sets the TagValue.
		/// </summary>
		internal protected Object TagValue 
		{
			get { return _tagValue; }
			set { _tagValue = value; }
		}
		/// <summary>
		/// Gets and sets the LogSocketReader.
		/// </summary>
		internal protected LogSocketReader LogSocketReader 
		{
			get { return _logSocketReader; }
			set { _logSocketReader = value; }
		}
		/// <summary>
		/// This called by an instance of LogReceiverLoggerFactory
		/// just prior to the LogSocketReader
		/// starting to read LogEntries from the Socket.
		/// </summary>
		/// <remarks>
		/// Subclasses can override this method to perform
		/// any initialization necessary. Alternatively, PreStart() can be overridden.
		/// </remarks>
		/// <param name="aFactory">The LogReceiverLoggerFactory that created this Logger.</param>
		internal protected virtual void PreStart(LogReceiverLoggerFactory aFactory) 
		{
			PreStart();
		}
		/// <summary>
		/// This called by PreStart(LogReceiverLoggerFactory)
		/// just prior to the LogSocketReader
		/// starting to read LogEntries from the Socket.
		/// </summary>
		/// <remarks>
		/// Subclasses can override this method to perform
		/// any initialization necessary. Alternatively, PreStart(LogReceiverLoggerFactory) can be overridden.
		/// </remarks>
		protected virtual void PreStart() 
		{
		}
		/// <summary>
		/// Create a new instance of LogReceiverLogger.
		/// </summary>
		/// <param name="aLogSocketReader">The LogSocketReader.</param>
		/// <param name="aTagValue">The TagValue.</param>
		public LogReceiverLogger(LogSocketReader aLogSocketReader, Object aTagValue) 
		{
			LogSocketReader = aLogSocketReader;
			TagValue = aTagValue;
		}
		/// <summary>
		/// Create a new instance of LogReceiverLogger.
		/// </summary>
		public LogReceiverLogger() 
		{
		}
	}
}
