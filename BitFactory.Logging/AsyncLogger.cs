/* 
 * (C) Copyright 2009 - Lorne Brinkman - All Rights Reserved.
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
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading;

namespace BitFactory.Logging
{
    /// <summary>
    /// An AsyncLogger instance can wrap any other logger, causing log entries to be written asynchronously in a background thread
    /// </summary>
    public class AsyncLogger : Logger
    {
        private static Thread Thread { get; set; }
        private static AutoResetEvent Signal { get; set; }
        private static List<KeyValuePair<Logger, LogEntry>> LogQueue { get; set; }

        private Logger InnerLogger { get; set; }

        static AsyncLogger()
        {
            Signal = new AutoResetEvent(false);
            LogQueue = new List<KeyValuePair<Logger, LogEntry>>();
            Thread = new Thread(new ThreadStart(LogWorker));
            Thread.IsBackground = true;
            Thread.Name = "Background Logging";
            Thread.Start();
        }

        private static void LogWorker()
        {
            while (true)
            {
                Signal.WaitOne();
                List<KeyValuePair<Logger, LogEntry>> queue = null;
                lock (LogQueue)
                {
                    queue = new List<KeyValuePair<Logger, LogEntry>>(LogQueue); //LogQueue.ToList();
                    LogQueue.Clear();
                }
                foreach (KeyValuePair<Logger, LogEntry> kv in queue)
                    try
                    {
                        kv.Key.Log(kv.Value);
                    }
                    catch(Exception ex)
                    {
                        OnLoggingError(kv.Key, "Async logging error", ex);
                    }
            }
        }

        /// <summary>
        /// Create an instance of AsyncLogger
        /// </summary>
        /// <param name="anInnerLogger">The logger being wrapped</param>
        public AsyncLogger(Logger anInnerLogger)
        {
            InnerLogger = anInnerLogger;            
        }

        /// <summary>
        /// Do log the LogEntry
        /// </summary>
        /// <param name="aLogEntry">The LogEntry being logged</param>
        /// <returns>The AsyncLogger always returns true</returns>
        protected internal override bool DoLog(LogEntry aLogEntry)
        {
            lock (LogQueue)
            {
                LogQueue.Add(new KeyValuePair<Logger, LogEntry>(InnerLogger, aLogEntry));
            }
            Signal.Set();
            return true;
        }
    }
}
