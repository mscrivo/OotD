using System.ComponentModel;
using System.Windows.Forms;
using System.Xml;

namespace OutlookDesktop
{
    class XmlSettings
    {
        #region Private Fields

        /// <summary>
        /// XmlDocument used to store the XmlData. 
        /// </summary>
        XmlDocument xmlDocument = new XmlDocument();

        /// <summary>
        /// Default path to save the settings. 
        /// </summary>
        string documentPath = Application.StartupPath + "//Data//settings.xml";
        #endregion

        #region Singleton Access Block
        public static XmlSettings Instance
        {
            get
            {
                return Nested.instance;
            }
        }

        class Nested
        {
            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static Nested()
            {
            }

            internal static readonly XmlSettings instance = new XmlSettings();
        }

        #endregion

        #region Constructor
        public XmlSettings()
        {
            try { xmlDocument.Load(documentPath); }
            catch { xmlDocument.LoadXml("<settings></settings>"); }
        }

        public XmlSettings(string xPath, object value)
        {
            try { xmlDocument.Load(documentPath); }
            catch { xmlDocument.LoadXml("<settings></settings>"); }

            PutSetting(xPath, value);
        }

        #endregion

        ~XmlSettings()
        {
            xmlDocument = null;
        }

        private string StripOutBadCharacters(string xPath)
        {
            //xPath = xPath.Replace("-", "_");
            ////xPath = xPath.Replace("7", "seven");
            //xPath = xPath.Replace("++", "plusplus");
            //xPath = xPath.Replace(" ", "_");

            return xPath;
        }

        public T GetSetting<T>(string xPath, T defaultValue)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + StripOutBadCharacters(xPath));
            if (xmlNode != null)
            {
                return (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromString(xmlNode.InnerText);
            }
            else { PutSetting(xPath, defaultValue); return defaultValue; }
        }

        public object GetSetting(string xPath, object defaultValue)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + StripOutBadCharacters(xPath));
            if (xmlNode != null)
            {
                return (object)TypeDescriptor.GetConverter(typeof(string)).ConvertFromString(xmlNode.InnerText);
            }
            else { PutSetting(xPath, defaultValue); return defaultValue; }
        }

        public void PutSetting(string xPath, object value)
        {
            XmlNode xmlNode = xmlDocument.SelectSingleNode("settings/" + StripOutBadCharacters(xPath));
            if (xmlNode == null) { xmlNode = createMissingNode("settings/" + StripOutBadCharacters(xPath)); }
            xmlNode.InnerText = TypeDescriptor.GetConverter(value.GetType()).ConvertToString(value);
            xmlDocument.Save(documentPath);
        }

        private XmlNode createMissingNode(string xPath)
        {
            string[] xPathSections = xPath.Split('/');
            string currentXPath = "";
            XmlNode testNode = null;
            XmlNode currentNode = xmlDocument.SelectSingleNode("settings");
            foreach (string xPathSection in xPathSections)
            {
                currentXPath += xPathSection;
                testNode = xmlDocument.SelectSingleNode(currentXPath);
                if (testNode == null)
                {
                    currentNode.InnerXml += "<" +
                                xPathSection + "></" +
                                xPathSection + ">";
                }
                currentNode = xmlDocument.SelectSingleNode(currentXPath);
                currentXPath += "/";
            }
            return currentNode;
        }
    }
}
