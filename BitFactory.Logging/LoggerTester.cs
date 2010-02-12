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
using System.Collections;
using System.Threading;

namespace BitFactory.Logging
{
	/// <summary>
	/// LoggerTester can be used to test Loggers.
	/// </summary>
	public class LoggerTester
	{
		/// <summary>
		/// A list of all running testers.
		/// </summary>
		private static ArrayList allLoggerTesters = new ArrayList();
		/// <summary>
		/// The logger to which this tester is logging.
		/// </summary>
		private Logger logger;
		/// <summary>
		/// The severity at which this tester logs.
		/// </summary>
		private LogSeverity logSeverity;
		/// <summary>
		/// The category of which this tester logs.
		/// </summary>
		private Object category;
		/// <summary>
		/// The Object this tester logs.
		/// </summary>
		private Object logItem;
		/// <summary>
		/// The thread on which this tester is logging.
		/// </summary>
		private Thread thread;
		/// <summary>
		/// Create a new instance of a LoggerTester.
		/// </summary>
		/// <param name="aLogger">The logger to which the tester should log.</param>
		/// <param name="aLogSeverity">The severity at which the tester should log.</param>
		/// <param name="aCategory">The category of which this tester should log.</param>
		/// <param name="aLogItem">The Object which this tester should log.</param>
		private LoggerTester(Logger aLogger, LogSeverity aLogSeverity, Object aCategory, Object aLogItem) 
		{
			logger = aLogger;
			logSeverity = aLogSeverity;
			category = aCategory;
			logItem = aLogItem;
			allLoggerTesters.Add(this);
			Start();
		}
		/// <summary>
		/// Start the logging thread.
		/// </summary>
		private void Start() 
		{
			thread = new Thread( new ThreadStart(Run));
			thread.IsBackground = true;
			thread.Start();
		}
		/// <summary>
		/// Stop the logging thread.
		/// </summary>
		private void Stop() 
		{
			thread.Abort();
		}		
		/// <summary>
		/// The logging thread start method.
		/// </summary>
		private void Run() 
		{
			try 
			{
				while (true) 
				{
					logger.Log(logSeverity,category,logItem);
					Thread.Sleep(1000);
				}
			}
			catch
			{
				allLoggerTesters.Remove(this);
			}
		}
		/// <summary>
		/// Create a tester for each severity and begin logging.
		/// </summary>
		/// <param name="aLogger">The logger to which the testers should log.</param>
		public static void TestAllSeverities(Logger aLogger) 
		{
			TestAllSeverities(aLogger, (Object)null);	// the cast forces the correct method call
		}
		/// <summary>
		/// Create a tester for each severity and begin logging.
		/// </summary>
		/// <param name="aLogger">The logger to which the testers should log.</param>
		/// <param name="aCategory">The category of which the testers should log.</param>
		public static void TestAllSeverities(Logger aLogger, Object aCategory) 
		{
			foreach (LogSeverity i in Enum.GetValues(typeof(LogSeverity)))
			{ 
				new LoggerTester(aLogger,i,aCategory,"This is a test");
				Thread.Sleep(250);
			}
		}
		/// <summary>
		/// Create a tester for each severity and each specified category and begin logging.
		/// </summary>
		/// <param name="aLogger">The logger to which the testers should log.</param>
		/// <param name="categories">The categories of which the testers should log.</param>
		public static void TestAllSeverities(Logger aLogger, Object[] categories) 
		{
			foreach (Object cat in categories)
				TestAllSeverities(aLogger, cat);
		}
		/// <summary>
		/// Stop all currently running testers.
		/// </summary>
		public static void StopAll() 
		{
			foreach (LoggerTester loggerTester in (ArrayList)allLoggerTesters.Clone())
				loggerTester.Stop();
		}
	}
}
