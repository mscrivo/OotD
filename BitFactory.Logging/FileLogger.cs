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
using System.IO;

namespace BitFactory.Logging
{
	/// <summary>
	/// Summary description for FileLogger.
	/// </summary>
	public class FileLogger : Logger
	{
		/// <summary>
		/// The name of the file to which this Logger is writing.
		/// </summary>
		private String _fileName;

		/// <summary>
		/// Gets and sets the file name.
		/// </summary>
		public String FileName 
		{
			get { return _fileName; }
			set
            {
                // support for relative paths                
                _fileName = Path.IsPathRooted(value)
                    ? value
                    : AppDomain.CurrentDomain.BaseDirectory + value.TrimStart('\\');
            }
		}
	
		/// <summary>
		/// Create a new instance of FileLogger.
		/// </summary>
		protected FileLogger() : base()
		{
		}
		/// <summary>
		/// Create a new instance of FileLogger.
		/// </summary>
		/// <param name="aFileName">The name of the file to which this Logger should write.</param>
		public FileLogger(String aFileName) : this() 
		{
			FileName = aFileName;
		}

		/// <summary>
		/// Create a new FileStream.
		/// </summary>
		/// <returns>The newly created FileStream.</returns>
		private FileStream CreateFileStream()
		{
			return new FileStream( FileName, FileMode.Append );
		}

		/// <summary>
		/// Get the FileStream.
		/// Create the directory structure if necessary.
		/// </summary>
		/// <returns>The FileStream.</returns>
		private FileStream GetFileStream()
		{
			try 
			{
				return CreateFileStream();
			}
			catch (DirectoryNotFoundException)
			{
				Directory.CreateDirectory((new FileInfo(FileName)).DirectoryName);
				return CreateFileStream();
			}
		}

		/// <summary>
		/// Create a new StreamWriter.
		/// </summary>
		/// <returns>A new StreamWriter.</returns>
		private StreamWriter GetStreamWriter() 
		{
			return new StreamWriter(GetFileStream());
		}

		/// <summary>
		/// Write the String to the file.
		/// </summary>
		/// <param name="s">The String representing the LogEntry being logged.</param>
		/// <returns>true upon success, false upon failure.</returns>
		protected override bool WriteToLog(String s) 
		{
			StreamWriter writer = null;
			try 
			{
				writer = GetStreamWriter();
				writer.WriteLine(s);
			} 
			catch(Exception ex)
			{
                OnLoggingError(this, "Error writing to file", ex);
				return false;
			} 
			finally 
			{
				try 
				{
					writer.Close();
				} 
				catch
				{
				}
			}
			return true;
		}

	}
}
