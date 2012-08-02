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
    /// The element representing a RollingFileLogger using a RolloverSizeStategy
    /// </summary>
    public class RollingSizeFileLoggerElement : LoggerElement
    {
        /// <summary>
        /// The path of the file
        /// </summary>
        [ConfigurationProperty("fileName", DefaultValue = "", IsRequired = true)]
        [Description("The path (and format string) of the name of the log file. This must include a format item {0} for the file. (e.g. c:\\logfiles\\myLog_{0}.log)")]
        public string FileName
        {
            get { return (string)this["fileName"]; }
            //set { this["fileName"] = value; }
        }

        /// <summary>
        /// The maximum size of the log file
        /// </summary>
        [ConfigurationProperty("maxSize", IsRequired = true)]
        [Description("The maximum size (in bytes) of the log file before rolling over to a new file")]
        public long MaxSize
        {
            get { return (long)this["maxSize"]; }
            //set { this["maxSize"] = value; }
        }
    }
}
