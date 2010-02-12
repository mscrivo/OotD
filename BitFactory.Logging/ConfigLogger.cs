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
using System.Text;
using System.Configuration;

using BitFactory.Logging.Configuration;

namespace BitFactory.Logging
{
    /// <summary>
    /// ConfigLogger is a self-configuring (by reading the config file) CompositeLogger subclass
    /// </summary>
    public class ConfigLogger : CompositeLogger
    {
        /// <summary>
        /// The automatically created (via config file), application-wide accessible logger
        /// </summary>
        public static readonly ConfigLogger Instance;

        private ConfigLogger()
        {
        }

        static ConfigLogger()
        {
            Instance = new ConfigLogger();
            var section = (LoggingSection) ConfigurationManager.GetSection("BitFactory.Logging");

            if (section.IsConfiguredForThisMachine())
            {
                Instance.Application = section.Application != ""
                    ? section.Application
                    : AppDomain.CurrentDomain.FriendlyName;

                Instance.Formatter = new LogEntryFormatStringFormatter(section.FormatString, Instance.OnLoggingError);
                Configure(Instance, Instance, section);
            }
        }


        private static Logger Configure(CompositeLogger aLogger, ConfigLogger aConfigLogger, CompositeLoggerElement aCompLoggerElement)
        {
            Configure((Logger)aLogger, aConfigLogger, aCompLoggerElement);

            foreach (CompositeLoggerElement element in aCompLoggerElement.CompositeLoggers)
                if (element.IsConfiguredForThisMachine())
                {
                    var logger = new CompositeLogger();
                    aLogger.AddLogger(element.Name, Configure(logger, Instance, element));
                }

            foreach (FileLoggerElement element in aCompLoggerElement.FileLoggers)
                if (element.IsConfiguredForThisMachine())
                {
                    var logger = new FileLogger(element.FileName);
                    logger.FileName = element.FileName;
                    aLogger.AddLogger(element.Name, Configure(logger, Instance, element));//, section));
                }

            foreach (RollingDateFileLoggerElement element in aCompLoggerElement.RollingDateFileLoggers)
                if (element.IsConfiguredForThisMachine())
                {
                    var logger = RollingFileLogger.NewRollingDateFileLogger(element.FormattedFileName);
                    aLogger.AddLogger(element.Name, Configure(logger, Instance, element));//, section));
                }

            foreach (RollingSizeFileLoggerElement element in aCompLoggerElement.RollingSizeFileLoggers)
                if (element.IsConfiguredForThisMachine())
                {
                    var logger = RollingFileLogger.NewRollingSizeFileLogger(element.FileName, element.MaxSize);
                    aLogger.AddLogger(element.Name, Configure(logger, Instance, element));//, section));
                }

            foreach (EmailLoggerElement element in aCompLoggerElement.EmailLoggers)
                if (element.IsConfiguredForThisMachine())
                {
                    var logger = new EmailLogger(new System.Net.Mail.SmtpClient(element.SmtpHost), element.From, element.To, element.Subject);
                    aLogger.AddLogger(element.Name, Configure(logger, Instance, element));//, section));
                }

            foreach (SocketLoggerElement element in aCompLoggerElement.SocketLoggers)
                if (element.IsConfiguredForThisMachine())
                {
                    var logger = new SerialSocketLogger(element.Host, element.Port);
                    aLogger.AddLogger(element.Name, Configure(logger, Instance, element));//, section));
                }

            foreach(ConsoleLoggerElement element in aCompLoggerElement.ConsoleLoggers)
                if (element.IsConfiguredForThisMachine())
                {
                    var logger = TextWriterLogger.NewConsoleLogger();
                    aLogger.AddLogger(element.Name, Configure(logger, Instance, element));
                }

            foreach(EventLogLoggerElement element in aCompLoggerElement.EventLogLoggers)
                if (element.IsConfiguredForThisMachine())
                {
                    var logger = new EventLogLogger(new System.Diagnostics.EventLog(element.LogName, Environment.MachineName, aConfigLogger.Application));
                    aLogger.AddLogger(element.Name, Configure(logger, Instance, element));
                }

            return aLogger;
        }


        private static Logger Configure(Logger aLogger, ConfigLogger aConfigLogger, LoggerElement aLoggerElement)
        {
            aLogger.Application = aConfigLogger.Application;

            var includeCategories = aLoggerElement.IncludeCategories.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var excludeCategories = aLoggerElement.ExcludeCategories.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            LogEntryFilter filter = null;

            if (includeCategories.Length > 0)
            {
                filter = new LogEntryCategoryFilter(true);
                foreach (string cat in includeCategories)
                    ((LogEntryCategoryFilter)filter).AddCategory(cat);
            }
            else if (excludeCategories.Length > 0)
            {
                filter = new LogEntryCategoryFilter(false);
                foreach (string cat in excludeCategories)
                    ((LogEntryCategoryFilter)filter).AddCategory(cat);
            }
            else
            {
                filter = new LogEntryPassFilter();
            }

            aLogger.Filter = filter;
            aLogger.SeverityThreshold = aLoggerElement.Severity;
            aLogger.Formatter = new LogEntryFormatStringFormatter((aLoggerElement.FormatString != "" ? aLoggerElement.FormatString : ((LogEntryFormatStringFormatter)aConfigLogger.Formatter).FormatString), aLogger.OnLoggingError);

            aLogger.Enabled = aLoggerElement.IsEnabled;

            var resultingLogger = aLoggerElement.IsInsistent
                ? new InsistentLogger(aLogger, 100, 180)
                : aLogger;

            resultingLogger = aLoggerElement.IsAsynchronous
                ? new AsyncLogger(resultingLogger)
                : resultingLogger;

            resultingLogger.Enabled = aLoggerElement.IsEnabled;

            return resultingLogger;
        }


    }
}
