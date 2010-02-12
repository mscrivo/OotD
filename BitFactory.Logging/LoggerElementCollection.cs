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

namespace BitFactory.Logging.Configuration
{
    /// <summary>
    /// A generic LoggerElementCollection
    /// </summary>
    /// <typeparam name="T">A LoggerElement subclass</typeparam>
    public abstract class LoggerElementCollection<T> : ConfigurationElementCollection
        where T : LoggerElement, new()
    {
        /// <summary>
        /// Create and return a new ConfigurationElement
        /// </summary>
        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        /// <summary>
        /// Return the element key.
        /// </summary>
        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((LoggerElement)element).Name;
        }

        /// <summary>
        /// Return the element with the given name
        /// </summary>
        public new T this[string aName]
        {
            get { return (T)BaseGet(aName); }
        }        
    }

    /// <summary>
    /// The CompositeLoggerElementCollection
    /// </summary>
    [ConfigurationCollection(typeof(CompositeLoggerElement), AddItemName = "compositeLogger", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class CompositeLoggerElementCollection : LoggerElementCollection<CompositeLoggerElement>
    {
    }

    /// <summary>
    /// The FileLoggerElementCollection
    /// </summary>
    [ConfigurationCollection(typeof(FileLoggerElement), AddItemName = "fileLogger", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class FileLoggerElementCollection : LoggerElementCollection<FileLoggerElement>
    {
    }

    /// <summary>
    /// The RollingDateFileLoggerElementCollection
    /// </summary>
    [ConfigurationCollection(typeof(RollingDateFileLoggerElement), AddItemName = "rollingDateFileLogger", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class RollingDateFileLoggerElementCollection : LoggerElementCollection<RollingDateFileLoggerElement>
    {
    }

    /// <summary>
    /// The RollingSizeFileLoggerElementCollection
    /// </summary>
    [ConfigurationCollection(typeof(RollingSizeFileLoggerElement), AddItemName = "rollingSizeFileLogger", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class RollingSizeFileLoggerElementCollection : LoggerElementCollection<RollingSizeFileLoggerElement>
    {
    }

    /// <summary>
    /// The EmailLoggerElementCollection
    /// </summary>
    [ConfigurationCollection(typeof(EmailLoggerElement), AddItemName = "emailLogger", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class EmailLoggerElementCollection : LoggerElementCollection<EmailLoggerElement>
    {
    }

    /// <summary>
    /// The SocketLoggerElementCollection
    /// </summary>
    [ConfigurationCollection(typeof(SocketLoggerElement), AddItemName = "socketLogger", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class SocketLoggerElementCollection : LoggerElementCollection<SocketLoggerElement>
    {
    }

    /// <summary>
    /// The EventLogLoggerElementCollection
    /// </summary>
    [ConfigurationCollection(typeof(EventLogLoggerElement), AddItemName = "eventLogLogger", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class EventLogLoggerElementCollection : LoggerElementCollection<EventLogLoggerElement>
    {
    }

    /// <summary>
    /// The ConsoleLoggerElementCollection
    /// </summary>
    [ConfigurationCollection(typeof(ConsoleLoggerElement), AddItemName = "consoleLogger", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ConsoleLoggerElementCollection : LoggerElementCollection<ConsoleLoggerElement>
    {
    }

}
