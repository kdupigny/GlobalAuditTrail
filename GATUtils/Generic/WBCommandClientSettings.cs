using System.Xml;

namespace GATUtils.Generic
{
    public class WBCommandClientSettings
    {
        public string LocalIp { get; private set; }
        public string RemoteIp { get; private set; }
        public int RemotePort { get; private set; }

        public WBCommandClientSettings(string localHost, string remoteHost, int remotePort)
        {
            LocalIp = localHost;
            RemoteIp = remoteHost;
            RemotePort = remotePort;
        }

        public WBCommandClientSettings(XmlNode settings)
        {
            foreach (XmlNode innerNode in settings)
            {
                switch (innerNode.Name.ToUpper())
                {
                    case "LOCALHOST":
                        LocalIp = innerNode.InnerText.Trim();
                        break;
                    case "TARGETHOST":
                        RemoteIp = innerNode.InnerText.Trim();
                        break;
                    case "TARGETPORT":
                        int temp = 8080;
                        int.TryParse(innerNode.InnerText.Trim(), out temp);
                        RemotePort = temp;
                        break;
                }
            }
        }
    }
}
