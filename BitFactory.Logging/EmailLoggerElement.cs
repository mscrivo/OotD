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
using System.Configuration;
using System.ComponentModel;

namespace BitFactory.Logging.Configuration
{
    /// <summary>
    /// The element representing an EmailLogger
    /// </summary>
    public class EmailLoggerElement : LoggerElement
    {
        /// <summary>
        /// The "to" email address
        /// </summary>
        [ConfigurationProperty("to", DefaultValue = "", IsRequired = true)]
        [Description("The email address to which log entries will be sent")]
        public string To
        {
            get { return (string)this["to"]; }
            set { this["to"] = value; }
        }

        /// <summary>
        /// The "from" email address
        /// </summary>
        [ConfigurationProperty("from", DefaultValue = "", IsRequired = true)]
        [Description("The email address from which log entries will be sent")]
        public string From
        {
            get { return (string)this["from"]; }
            set { this["from"] = value; }
        }

        /// <summary>
        /// The SMTP host name
        /// </summary>
        [ConfigurationProperty("smtpHost", DefaultValue = "", IsRequired = false)]
        [Description("The SMTP host for sending emails")]
        public string SmtpHost
        {
            get { return (string)this["smtpHost"]; }
            set { this["smtpHost"] = value; }
        }

        /// <summary>
        /// The base subject of the email
        /// </summary>
        [ConfigurationProperty("subject", DefaultValue = "", IsRequired = false)]
        [Description("The subject of the email. Ths can be a format string in the same format as the FormatString.")]
        public string Subject
        {
            get { return (string)this["subject"]; }
            set { this["subject"] = value; }
        }
    }
}
