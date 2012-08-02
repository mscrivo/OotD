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
    /// The element representing a SerialSocketLogger
    /// </summary>
    public class SocketLoggerElement : LoggerElement
    {
        /// <summary>
        /// The name of the host with the open socket
        /// </summary>
        [ConfigurationProperty("host", DefaultValue = "localhost", IsRequired = true)]
        [Description("The name (or IP) of the host with the open server socket")]
        public string Host
        {
            get { return (string)this["host"]; }
            //set { this["host"] = value; }
        }

        /// <summary>
        /// The port number of the socket
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = 0, IsRequired = true)]
        [Description("The port number of the server socket")]
        public int Port
        {
            get { return (int)this["port"]; }
            //set { this["port"] = value; }
        }
    }
}
