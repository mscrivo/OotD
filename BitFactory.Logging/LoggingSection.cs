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
    /// The Logging Section of a configuration file--essentially representing a CompositeLogger
    /// </summary>    
    public class LoggingSection : CompositeLoggerElement
    {
        /// <summary>
        /// Handle the unrecognized 'xmlns' attribute
        /// </summary>
        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            if (name == "xmlns")
                return true;

            return base.OnDeserializeUnrecognizedAttribute(name, value);
        }

        /// <summary>
        /// The Application name
        /// </summary>
        [ConfigurationProperty("application", DefaultValue = "", IsRequired = false)]
        [Description("The name of the application. Leave it blank to use the name of the exe file automatically.")]
        public string Application
        {
            get { return (string)this["application"]; }
        }

        /// <summary>
        /// The FormatString for the Logger
        /// </summary>
        [ConfigurationProperty("formatString", DefaultValue = "[{application}] - [{severity}] - [{timestamp}] - {message}", IsRequired = false)]
        [Description("The format string of the logger. If blank, it will use the format string of the enclosing section (the CompositeLogger).")]
        public override string FormatString
        {
            get { return (string)this["formatString"]; }
        }

    }
}
