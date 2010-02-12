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
    /// The base element representing a Logger
    /// </summary>
    public class LoggerElement : ConfigurationSection //ConfigurationElement
    {
        /// <summary>
        /// Occurs after the element is deserialized
        /// </summary>
        protected override void PostDeserialize()
        {
            base.PostDeserialize();
            Validate();
        }

        /// <summary>
        /// Validate the element. Throw a exception if not valid.
        /// </summary>
        protected virtual void Validate()
        {
            if ((IncludeCategories.Trim() != "") && (ExcludeCategories.Trim() != ""))
                throw new ConfigurationErrorsException("logging element can have either includeCategories or excludeCategories, but not both.");            
        }

        /// <summary>
        /// Return true if the LoggerElement is configured for the current machine; otherwise return false.
        /// </summary>
        public bool IsConfiguredForThisMachine()
        {
            var machineNames = Machine.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            return (machineNames.Length == 0) || new List<string>(machineNames).Exists(name => name.Equals(Environment.MachineName, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// The name of the Logger
        /// </summary>
        [ConfigurationProperty("name", DefaultValue = "", IsKey = true, IsRequired = true)]
        [Description("The name of the logger")]
        public string Name
        {
            get { return (string)this["name"]; }
        }

        /// <summary>
        /// The machine names (separated by commas) for which the logger will be created. An empty value creates it on all machines.
        /// </summary>
        [ConfigurationProperty("machine", DefaultValue = "", IsRequired = false)]
        [Description("The machine names (separated by commas) for which this logger will be created. Leaving it empty will create it on all machines")]
        public string Machine
        {
            get { return (string)this["machine"]; }
        }

        /// <summary>
        ///  if the Logger is enabled, otherwise false.
        /// </summary>
        [ConfigurationProperty("isEnabled", DefaultValue = true, IsRequired = false)]
        [Description("true if the logger is enabled; otherwise false")]
        public bool IsEnabled
        {
            get { return (bool)this["isEnabled"]; }
        }

        /// <summary>
        /// The SeverityThreshold of the Logger
        /// </summary>
        [ConfigurationProperty("severity", DefaultValue = LogSeverity.Debug, IsRequired = false)]
        [Description("The SeverityThreshold of the logger")]
        public LogSeverity Severity
        {
            get { return (LogSeverity)this["severity"]; }
        }

        /// <summary>
        /// Categories to include (leave blank for all)
        /// </summary>
        [ConfigurationProperty("includeCategories", DefaultValue = "", IsRequired = false)]
        [Description("The categories, separated by commas, to include when logging. Leave blank to include everything.")]
        public string IncludeCategories
        {
            get { return (string)this["includeCategories"]; }
        }

        /// <summary>
        /// Categories to exclude
        /// </summary>
        [ConfigurationProperty("excludeCategories", DefaultValue = "", IsRequired = false)]
        [Description("The categories, separated by commas, to exclude when logging.")]
        public string ExcludeCategories
        {
            get { return (string)this["excludeCategories"]; }
        }

        /// <summary>
        /// If true, wrap the Logger in an InsistentLogger.
        /// </summary>
        [ConfigurationProperty("isInsistent", DefaultValue = false, IsRequired = false)]
        [Description("if true, the logger will be wrapped in an InsistentLogger")]
        public bool IsInsistent
        {
            get { return (bool)this["isInsistent"]; }
        }

        /// <summary>
        /// If true, wrap the Logger in an AsyncLogger.
        /// </summary>
        [ConfigurationProperty("isAsynchronous", DefaultValue = false, IsRequired = false)]
        [Description("if true, the logger will be wrapped in an AsyncLogger")]
        public bool IsAsynchronous
        {
            get { return (bool)this["isAsynchronous"]; }
        }

        /// <summary>
        /// The FormatString for the Logger
        /// </summary>
        [ConfigurationProperty("formatString", DefaultValue = "", IsRequired = false)]
        [Description("The format string of the logger. If blank, it will use the format string of the enclosing section (the CompositeLogger).")]
        public virtual string FormatString
        {
            get { return (string)this["formatString"]; }
        }

    }
}
