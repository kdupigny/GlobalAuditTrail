using System.Xml;

namespace GATUtils.Generic
{
    public class WbCommandServerSettings
    {
        public string LocalIp { get; private set; }
        public int LocalPort { get; private set; }
        public WBCommandServerPresets PresetSettings { get; private set; }

        public WbCommandServerSettings(string localHost, int port)
        {
            LocalIp = localHost;
            LocalPort = port;
        }

        public WbCommandServerSettings(XmlDocument settingsDocument)
        {
            XmlNode settings = settingsDocument.GetElementsByTagName("CommandServerDetails").Item(0);
            if (settings != null)
                foreach (XmlNode innerNode in settings)
                {
                    switch (innerNode.Name.ToUpper())
                    {
                        case "LOCALHOST":
                            LocalIp = innerNode.InnerText.Trim();
                            break;
                        case "LOCALPORT":
                            int temp = 8080;
                            int.TryParse(innerNode.InnerText.Trim(), out temp);
                            LocalPort = temp;
                            break;
                    }
                }

            PresetSettings = new WBCommandServerPresets(settingsDocument.GetElementsByTagName("PresetCommand"));
        }
    }
}
