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
using System.ComponentModel;

namespace BitFactory.Logging.Configuration
{
    /// <summary>
    /// The element representing a CompositeLogger
    /// </summary>
    public class CompositeLoggerElement : LoggerElement
    {
        /// <summary>
        /// A collection of CompositeLoggerElements
        /// </summary>
        [ConfigurationProperty("compositeLoggers")]
        public CompositeLoggerElementCollection CompositeLoggers
        {
            get { return (CompositeLoggerElementCollection)this["compositeLoggers"]; }
        }


        /// <summary>
        /// A collection of FileLoggerElements
        /// </summary>
        [ConfigurationProperty("fileLoggers")]
        public FileLoggerElementCollection FileLoggers
        {
            get { return (FileLoggerElementCollection)this["fileLoggers"]; }
        }

        /// <summary>
        /// A collection of RollingDateFileLoggerElements
        /// </summary>
        [ConfigurationProperty("rollingDateFileLoggers")]
        public RollingDateFileLoggerElementCollection RollingDateFileLoggers
        {
            get { return (RollingDateFileLoggerElementCollection)this["rollingDateFileLoggers"]; }
        }

        /// <summary>
        /// A collection of RollingSizeFileLoggerElements
        /// </summary>
        [ConfigurationProperty("rollingSizeFileLoggers")]
        public RollingSizeFileLoggerElementCollection RollingSizeFileLoggers
        {
            get { return (RollingSizeFileLoggerElementCollection)this["rollingSizeFileLoggers"]; }
        }

        /// <summary>
        /// A collection of EmailLoggerElements
        /// </summary>
        [ConfigurationProperty("emailLoggers")]
        public EmailLoggerElementCollection EmailLoggers
        {
            get { return (EmailLoggerElementCollection)this["emailLoggers"]; }
        }

        /// <summary>
        /// A collection of SocketLoggerElements
        /// </summary>
        [ConfigurationProperty("socketLoggers")]
        public SocketLoggerElementCollection SocketLoggers
        {
            get { return (SocketLoggerElementCollection)this["socketLoggers"]; }
        }

        /// <summary>
        /// A collection of EventLogLoggerElements
        /// </summary>
        [ConfigurationProperty("eventLogLoggers")]
        public EventLogLoggerElementCollection EventLogLoggers
        {
            get { return (EventLogLoggerElementCollection)this["eventLogLoggers"]; }
        }

        /// <summary>
        /// A collection of ConsoleLoggerElements
        /// </summary>
        [ConfigurationProperty("consoleLoggers")]
        public ConsoleLoggerElementCollection ConsoleLoggers
        {
            get { return (ConsoleLoggerElementCollection)this["consoleLoggers"]; }
        }
    }
}
