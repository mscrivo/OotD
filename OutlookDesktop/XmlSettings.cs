using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;

namespace OutlookDesktop
{
    internal class XmlSettings
    {
        #region Private Fields

        /// <summary>
        /// Default path to save the settings. 
        /// </summary>
        private readonly string _documentPath = Application.StartupPath + "//Data//settings.xml";

        /// <summary>
        /// XmlDocument used to store the XmlData. 
        /// </summary>
        private XmlDocument _xmlDocument = new XmlDocument();

        #endregion

        #region Singleton Access Block

        public static XmlSettings Instance
        {
            get { return Nested.instance; }
        }

        private class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit

            internal static readonly XmlSettings instance = new XmlSettings();
        }

        #endregion

        #region Constructor

        public XmlSettings()
        {
            try
            {
                _xmlDocument.Load(_documentPath);
            }
            catch
            {
                _xmlDocument.LoadXml("<settings></settings>");
            }
        }

        public XmlSettings(string xPath, object value)
        {
            try
            {
                _xmlDocument.Load(_documentPath);
            }
            catch
            {
                _xmlDocument.LoadXml("<settings></settings>");
            }

            PutSetting(xPath, value);
        }

        #endregion

        ~XmlSettings()
        {
            _xmlDocument = null;
        }

        private static string StripOutBadCharacters(string xPath)
        {
            //xPath = xPath.Replace("-", "_");
            ////xPath = xPath.Replace("7", "seven");
            //xPath = xPath.Replace("++", "plusplus");
            //xPath = xPath.Replace(" ", "_");

            return xPath;
        }

        public T GetSetting<T>(string xPath, T defaultValue)
        {
            XmlNode xmlNode = _xmlDocument.SelectSingleNode("settings/" + StripOutBadCharacters(xPath));
            if (xmlNode != null)
            {
                return (T) TypeDescriptor.GetConverter(typeof (T)).ConvertFromString(xmlNode.InnerText);
            }
            else
            {
                PutSetting(xPath, defaultValue);
                return defaultValue;
            }
        }

        public object GetSetting(string xPath, object defaultValue)
        {
            XmlNode xmlNode = _xmlDocument.SelectSingleNode("settings/" + StripOutBadCharacters(xPath));
            
            if (xmlNode != null)
            {
                return TypeDescriptor.GetConverter(typeof (string)).ConvertFromString(xmlNode.InnerText);
            }
            
            PutSetting(xPath, defaultValue);
            return defaultValue;
        }

        public void PutSetting(string xPath, object value)
        {
            XmlNode xmlNode = _xmlDocument.SelectSingleNode("settings/" + StripOutBadCharacters(xPath));
            
            if (xmlNode == null)
            {
                xmlNode = CreateMissingNode("settings/" + StripOutBadCharacters(xPath));
            }

            xmlNode.InnerText = TypeDescriptor.GetConverter(value.GetType()).ConvertToString(value);
            _xmlDocument.Save(_documentPath);
        }

        private XmlNode CreateMissingNode(string xPath)
        {
            string[] xPathSections = xPath.Split('/');
            string currentXPath = "";
            XmlNode testNode = null;
            XmlNode currentNode = _xmlDocument.SelectSingleNode("settings");
            foreach (string xPathSection in xPathSections)
            {
                currentXPath += xPathSection;
                testNode = _xmlDocument.SelectSingleNode(currentXPath);
                if (testNode == null)
                {
                    currentNode.InnerXml += "<" +
                                            xPathSection + "></" +
                                            xPathSection + ">";
                }
                currentNode = _xmlDocument.SelectSingleNode(currentXPath);
                currentXPath += "/";
            }
            return currentNode;
        }
    }
}