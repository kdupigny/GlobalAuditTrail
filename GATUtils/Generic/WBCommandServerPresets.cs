using System.Collections.Generic;
using System.Xml;

namespace GATUtils.Generic
{
    public class WBCommandServerPresets
    {
        public Dictionary<string, Dictionary<string, string>> PresetCommands { get { return _presets ?? (_presets = new Dictionary<string, Dictionary<string, string>>()); } }

        public string this[string presetCommandName, string attribute]
        {
            get
            {
                Dictionary<string, string> temp;
                _presets.TryGetValue(presetCommandName, out temp);
                
                string attributeValue;
                temp.TryGetValue(attribute, out attributeValue);
                return attributeValue;
            }
        }

        public WBCommandServerPresets(XmlNodeList presetSettings)
        {
            foreach (XmlNode presetCommand in presetSettings)
            {
                string currentNodeName = string.Empty;// = presetCommand.InnerText.Trim();
                Dictionary<string, string> attributes = new Dictionary<string, string>();
                foreach (XmlNode node in presetCommand)
                {
                    switch (node.Name.ToUpper())
                    {
                        case "NAME":
                            currentNodeName = node.InnerText.Trim();
                            break;
                        default:
                            attributes.Add(node.Name.ToUpper(), node.InnerText.Trim());
                            break;
                    }
                }

                PresetCommands.Add(currentNodeName, attributes);
            }
        }

        private Dictionary<string, Dictionary<string, string>> _presets;
    }
}