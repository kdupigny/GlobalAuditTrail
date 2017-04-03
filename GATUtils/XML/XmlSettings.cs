using System;
using System.Xml;
using System.IO;
using GATUtils.Generic;
using GATUtils.Utilities.FileUtils;

namespace GATUtils.XML
{
    public class XmlSettings
    {
        public static XmlSettings Instance
        {
            get { return s_instance ?? (s_instance = new XmlSettings()); }
        }

        public XmlDocument SettingDocument { get { return _sSettingsDocument; } }

        public string ApplicationName { get { return (string.IsNullOrEmpty(_appName) ? "GatApplication" : _appName); } }
        public TimeSpan ApplicationStartTime { get; private set; }
        public TimeSpan ApplicationEndTime { get; private set; }
        public GatLoggerSettings GatLogger;
        public DBSettings Db;
        public WbEmailSettings WbEmail;
        public WBCommandClientSettings WbCommandClient;
        public WbCommandServerSettings WbCommandServer;
        public int DaysBack;
        public DateTime SpecificRunDate { get { return _specificDate; } set { _specificDate = value; } }

        public bool AutoConnectFixSessions { get; private set; }
        public bool AutoConnectClientFixSessions { get; private set; }

        public XmlSettings()
        {
            _xmlFilePath = GatFile.Path(Dir.ApplicationSettings, _xmlFilePath);
            _ReadXmlSettings();
        }

        public void LoadXmlSettings(string path)
        {
            _xmlFilePath = path;
            _ReadXmlSettings();
        }

        public bool Validate()
        {
            if (s_instance != null)
                return true;

            return false;
        }
        
        private void _ReadXmlSettings()
        {
            if (File.Exists(_xmlFilePath))
            {
                _sSettingsDocument = new XmlDocument();
                _sSettingsDocument.Load(_xmlFilePath);

                XmlNode tempNode = _sSettingsDocument.GetElementsByTagName("AppName").Item(0);
                _appName = tempNode != null ? tempNode.InnerText : "GatApplication";

                tempNode = _sSettingsDocument.GetElementsByTagName("AppStartTime").Item(0);
                ApplicationStartTime = tempNode != null ? TimeSpan.Parse(tempNode.InnerText) : TimeSpan.Zero;

                tempNode = _sSettingsDocument.GetElementsByTagName("AppEndTime").Item(0);
                ApplicationEndTime = tempNode != null ? TimeSpan.Parse(tempNode.InnerText) : TimeSpan.Zero;

                tempNode = _sSettingsDocument.GetElementsByTagName("DaysBack").Item(0);
                if (tempNode != null) int.TryParse(tempNode.InnerText, out DaysBack);

                tempNode = _sSettingsDocument.GetElementsByTagName("SpecificDate").Item(0);
                if (tempNode != null) 
                    if (!DateTime.TryParse(tempNode.InnerText, out _specificDate)) _specificDate = DateTime.Now;

                GatLogger = new GatLoggerSettings(_sSettingsDocument.GetElementsByTagName("Logging"));

                tempNode = _sSettingsDocument.GetElementsByTagName("CommandServerDetails").Item(0);
                if (tempNode != null) WbCommandServer = new WbCommandServerSettings(_sSettingsDocument);

                //_GetCommandServerSettings(ref _sSettingsDocument);

                tempNode = _sSettingsDocument.GetElementsByTagName("CommandClientDetails").Item(0);
                if(tempNode != null) WbCommandClient = new WBCommandClientSettings(tempNode);

                //_GetCommandClientSettings(ref _sSettingsDocument);
                
                _GetActiveDb(_sSettingsDocument.GetElementsByTagName("DbSettings"));
                _LoadFixSettings(_sSettingsDocument);
                
                WbEmail = new WbEmailSettings(_sSettingsDocument);
            }
            else
            {
                _sSettingsDocument = null;
                GatLogger = new GatLoggerSettings(null);
            }
        }

        /// <summary>
        /// Gets the first active DB found in the applications settings.
        /// </summary>
        /// <param name="dbConfigurations">The db configurations.</param>
        /// <returns></returns>
        private void _GetActiveDb(XmlNodeList dbConfigurations)
        {
            DBSettings temp;
            foreach (XmlNode dbConfig in dbConfigurations)
            {
                temp = new DBSettings(dbConfig);
                if (temp.IsActive)
                {
                    Db = temp;
                    return;
                }
            }
        }

        private void _GetCommandServerSettings(ref XmlDocument settingsDocument)
        {
            XmlNodeList serverNodes = settingsDocument.GetElementsByTagName("CommandSeverDetails");
            XmlNode commandServerNode = serverNodes.Item(0);
            if (commandServerNode != null) WbCommandServer = new WbCommandServerSettings(settingsDocument);
        }

        private void _GetCommandClientSettings(ref XmlDocument settingsDocument)
        {
            XmlNode commandClientNode = settingsDocument.GetElementsByTagName("CommandClientDetails").Item(0);
            if (commandClientNode != null) WbCommandClient = new WBCommandClientSettings(commandClientNode);
        }

        private void _LoadFixSettings(XmlDocument settingsDocument)
        {
            XmlNode fixSettingsNode = settingsDocument.GetElementsByTagName("Fix").Item(0);
            if (fixSettingsNode != null)
                foreach (XmlNode node in fixSettingsNode)
                {
                    switch (node.Name.ToUpper())
                    {
                        case "AUTOCONNECT":
                            AutoConnectFixSessions = XmlTools.ResolveBooleanValue(node.InnerText.Trim());
                            break;
                        case "AUTOCONNECTCLIENTS":
                            AutoConnectClientFixSessions = XmlTools.ResolveBooleanValue(node.InnerText.Trim());
                            break;
                    }
                }
        }

        private static XmlSettings s_instance;
        private XmlDocument _sSettingsDocument;

        private DateTime _specificDate = DateTime.Now;

        private string _appName;
        private string _xmlFilePath = "ApplicationSettings.xml";
    }
}
