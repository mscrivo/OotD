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
	/// Represent severities of LogEntries.
	/// </summary>
	public enum LogSeverity
	{
		/// <summary>
		/// Represents a severity level of "Debug"
		/// </summary>
		Debug = 1,
		/// <summary>
		/// Represents a severity level of "Info"
		/// </summary>
		Info = 2,
		/// <summary>
		/// Represents a severity level of "Status"
		/// </summary>
		Status = 3,
		/// <summary>
		/// Represents a severity level of "Warning"
		/// </summary>
		Warning = 4,
		/// <summary>
		/// Represents a severity level of "Error"
		/// </summary>
		Error = 5,
		/// <summary>
		/// Represents a severity level of "Critical"
		/// </summary>
		Critical = 6,
		/// <summary>
		/// Represents a severity level of "Fatal"
		/// </summary>
		Fatal = 7,
	}
}
